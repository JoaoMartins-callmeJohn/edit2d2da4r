using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using DesignAutomationFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Document = Autodesk.Revit.DB.Document;

namespace Autodesk.Forge.Sample.DesignAutomation.Revit
{
	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class Commands : IExternalDBApplication
	{
		//Path of the project(i.e)project where your Window family files are present
		string OUTPUT_FILE = "OutputFile.rvt";

		public ExternalDBApplicationResult OnStartup(ControlledApplication application)
		{
			DesignAutomationBridge.DesignAutomationReadyEvent += HandleDesignAutomationReadyEvent;
			return ExternalDBApplicationResult.Succeeded;
		}

		private void HandleDesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
		{
			LogTrace("Design Automation Ready event triggered...");
			e.Succeeded = true;
			CreatePolyLine(e.DesignAutomationData.RevitDoc);
		}

		private void CreatePolyLine(Document revitDoc)
		{
			InputParams inputParameters = JsonConvert.DeserializeObject<InputParams>(File.ReadAllText("params.json"));

			using (Transaction trans = new Transaction(revitDoc))
			{
				trans.Start("Create Curves");

				Console.WriteLine("Reading input points");
				List<XYZ> points = inputParameters.Points.Select(point => new XYZ(point.X, point.Y, point.Z)).ToList();
				Console.WriteLine($"{points.Count} points found!");
				points.ForEach(point => Console.WriteLine(point.ToString()));

				Console.WriteLine("Acquiring View!");
				List<Element> views = new FilteredElementCollector(revitDoc).OfClass(typeof(View)).WhereElementIsNotElementType().ToElements().ToList();

				View inputView = views.Find(v => v.Name == inputParameters.ViewName) as View;

				Console.WriteLine($"View {inputView.Name} found!");

				Console.WriteLine("Creating Curves!");
				for (int i = 0; i < points.Count - 1; i++)
				{
					Line newLine = Line.CreateBound(points[i], points[i + 1]);
					revitDoc.Create.NewModelCurve(newLine, inputView.SketchPlane);
				}
				Console.WriteLine("Curves Created!");

				trans.Commit();
			}

			//Save the updated file by overwriting the existing file
			ModelPath ProjectModelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(OUTPUT_FILE);
			SaveAsOptions SAO = new SaveAsOptions();
			SAO.OverwriteExistingFile = true;

			//Save the project file with updated window's parameters
			LogTrace("Saving file...");
			revitDoc.SaveAs(ProjectModelPath, SAO);
		}
		public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
		{
			return ExternalDBApplicationResult.Succeeded;
		}

		public class InputParams
		{
			public string ViewName { get; set; }
			public List<InputPoint> Points { get; set; }
		}

		public class InputPoint
		{
			public double X { get; set; }	
			public double Y { get; set; }
			public double Z { get; set; }
		}

		/// <summary>
		/// This will appear on the Design Automation output
		/// </summary>
		private static void LogTrace(string format, params object[] args) { System.Console.WriteLine(format, args); }
	}
}

