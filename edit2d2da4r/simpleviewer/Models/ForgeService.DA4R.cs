using System.Dynamic;
using Autodesk.Forge;
using Autodesk.Forge.Client;
using Autodesk.Forge.DesignAutomation;
using Autodesk.Forge.DesignAutomation.Model;
using Autodesk.Forge.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static simpleviewer.Controllers.DA4RController;

public partial class ForgeService
{
	public static string _uploadKey;
	public static bool HttpErrorHandler(ApiResponse<dynamic> response, string msg = "", bool bThrowException = true)
	{
		if (response.StatusCode < 200 || response.StatusCode >= 300)
		{
			if (bThrowException)
				throw new Exception(msg + " (HTTP " + response.StatusCode + ")");
			return (true);
		}
		return (false);
	}
	private async static Task<string> PrepareInputUrl(string bucketKey, string objectKey, Token oauth)
	{
		try
		{
			ObjectsApi objectsAPI = new ObjectsApi();
			objectsAPI.Configuration.AccessToken = oauth.AccessToken;
			ApiResponse<dynamic> response = await objectsAPI.getS3DownloadURLAsyncWithHttpInfo(bucketKey, objectKey, new Dictionary<string, object> {
						{ "minutesExpiration", 60.0 },
						{ "useCdn", true }
				});
			HttpErrorHandler(response, $"Failed to get S3 download url");
			return response.Data.url;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Exception when preparing input url:{ex.Message}");
			throw;
		}
	}
	private static async Task<string> PrepareOutputUrl(string bucketKey, string objectKey, Token oauth)
	{
		try
		{
			ObjectsApi objectsAPI = new ObjectsApi();
			objectsAPI.Configuration.AccessToken = oauth.AccessToken;

			ApiResponse<dynamic> response = await objectsAPI.getS3UploadURLAsyncWithHttpInfo(bucketKey, objectKey,
							new Dictionary<string, object> {
						{ "minutesExpiration", 60.0 },/*Kept large value intentionally*/
            { "useCdn", true } /*to get cloudfront url*/
							});
			HttpErrorHandler(response, $"Failed to get S3 upload url");
			//We need s3 upload payload to finalize the upload
			PostCompleteS3UploadPayload payload = new PostCompleteS3UploadPayload(response.Data.uploadKey, null);
			_uploadKey = response.Data["uploadKey"];
			string url = response.Data["urls"][0];
			return (url);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to prepare output argument :{ex.Message}");
			throw;
		}
	}

	public async Task<dynamic> StartWorkitem(WorkItemParams workItemParams, DesignAutomationClient _designAutomation)
	{
		// basic input validation
		string activityName = "JMONDATADAYS.DrawCurvesActivity+second";

		// OAuth token
		Token oauth = await this.GetToken(new Scope[] {Scope.CodeAll, Scope.DataWrite, Scope.DataCreate, Scope.DataRead});

		string bucketKey = workItemParams.urn.Split(':')[3].Split('/')[0];
		string objectKey = workItemParams.urn.Split(':')[3].Split('/')[1];

		string inputUrl = await PrepareInputUrl(bucketKey, objectKey, oauth);

		// prepare workitem arguments
		// 1. input file
		XrefTreeArgument inputFileArgument = new XrefTreeArgument()
		{
			Url = inputUrl
		};

		// 2. input json
		dynamic inputJson = new JObject();
		inputJson.ViewName = workItemParams.viewname;
		inputJson.Points = JArray.FromObject(workItemParams.points);
		XrefTreeArgument inputJsonArgument = new XrefTreeArgument()
		{
			Url = "data:application/json," + ((JObject)inputJson).ToString(Formatting.None).Replace("\"", "'").Replace(@"\", "")
		};
		// 3. output file
		string outputFileNameOSS = string.Format("{0}_output_{1}", DateTime.Now.ToString("yyyyMMddhhmmss"), Path.GetFileName(objectKey)); // avoid overriding
		string outputUrl = await PrepareOutputUrl(bucketKey, outputFileNameOSS, oauth);
		XrefTreeArgument outputFileArgument = new XrefTreeArgument()
		{
			Url = outputUrl,
			Verb = Verb.Put
		};

		// prepare & submit workitem
		WorkItem workItemSpec = new WorkItem()
		{
			ActivityId = activityName,
			Arguments = new Dictionary<string, IArgument>()
			{
				{ "inputFile", inputFileArgument },
				{ "inputJson",  inputJsonArgument },
				{ "result", outputFileArgument }
			}
		};

		WorkItemStatus workItemStatus = await _designAutomation.CreateWorkItemAsync(workItemSpec);

		dynamic response = new ExpandoObject();
		response.workitemId = workItemStatus.Id;
		response.uploadKey = _uploadKey;
		response.bucketKey = bucketKey;
		response.objectName = outputFileNameOSS;

		return response;
	}

	public async Task<dynamic> GetWorkitem(string workitemId, string uploadkey, string bucketKey, string objectName, DesignAutomationClient _designAutomation)
	{
		WorkItemStatus workItemStatus = await _designAutomation.GetWorkitemStatusAsync(workitemId);

		dynamic workitemReturn = new ExpandoObject();
		workitemReturn.status = workItemStatus.Status.ToString();
		workitemReturn.reportUrl = workItemStatus.ReportUrl==null ? "" : workItemStatus.ReportUrl.ToString();

		if (workItemStatus.Status == Status.Success)
			CompleteUpload(uploadkey, bucketKey, objectName);

		return workitemReturn;
	}

	public async Task CompleteUpload(string uploadkey, string bucketKey, string objectName)
	{
		Token oauth = await this.GetToken(new Scope[] { Scope.DataWrite, Scope.DataCreate });

		ObjectsApi objectsApi = new ObjectsApi();
		objectsApi.Configuration.AccessToken = oauth.AccessToken;

		//finalize upload in the callback.
		PostCompleteS3UploadPayload payload = new PostCompleteS3UploadPayload(uploadkey, null);
		ApiResponse<dynamic> res = await objectsApi.completeS3UploadAsyncWithHttpInfo(bucketKey, objectName, payload, new Dictionary<string, object> {
				{ "minutesExpiration", 2.0 },
				{ "useCdn", true }
				});
	}
}