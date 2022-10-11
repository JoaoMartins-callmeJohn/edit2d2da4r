using Autodesk.Forge.Core;
using Autodesk.Forge.DesignAutomation;
using Autodesk.Forge.DesignAutomation.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static AuthController;
using static ModelsController;

namespace simpleviewer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DA4RController : ControllerBase
	{
		private readonly ForgeService _forgeService;
		private DesignAutomationClient _designAutomation;

		public DA4RController(ForgeService forgeService)
		{
			_forgeService = forgeService;
			Autodesk.Forge.Core.ForgeService service =
								new Autodesk.Forge.Core.ForgeService(
										new HttpClient(
												new ForgeHandler(Microsoft.Extensions.Options.Options.Create(new ForgeConfiguration()
												{
													ClientId = forgeService._clientId,
													ClientSecret = forgeService._clientSecret
												}))
												{
													InnerHandler = new HttpClientHandler()
												})
								);
			_designAutomation = new DesignAutomationClient(service);
		}

		public class Point
		{
			public double X { get; set; }
			public double Y { get; set; }
			public double Z { get; set; }
		}

		public class WorkItemParams
		{
			[FromBody]
			public List<Point> points { get; set; }

			[FromBody]
			public string viewname { get; set; }

			[FromBody]
			public string urn { get; set; }
		}

		[HttpPost("workitem")]
		public async Task<dynamic> SubmitWorkitem([FromBody] WorkItemParams workItemParams)
		{
			dynamic workitemInfo = await _forgeService.StartWorkitem(workItemParams, _designAutomation);
			return Ok(new { WorkItemId = workitemInfo.workitemId, UploadKey = workitemInfo.uploadKey, BucketKey = workitemInfo.bucketKey, ObjectName = workitemInfo.objectName });
		}

		[HttpGet("workitem")]
		public async Task<dynamic> CheckWorkitem(string workitemId, string uploadkey, string bucketKey, string objectName)
		{
			dynamic status = await _forgeService.GetWorkitem(workitemId, uploadkey, bucketKey, objectName, _designAutomation);
			return Ok(new { Status = status.ToString() });
		}
	}
}
