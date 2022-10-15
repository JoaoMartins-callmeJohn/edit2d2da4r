# Revit ModelCurve Creator bundle for APS (formerly Forge) Design Automation

[![Design Automation](https://img.shields.io/badge/Design%20Automation-v3-green.svg)](http://developer.autodesk.com/)

![Windows](https://img.shields.io/badge/Plugins-Windows-lightgrey.svg)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.8-blue.svg)
[![Revit-2022](https://img.shields.io/badge/Revit-2022-lightgrey.svg)](http://autodesk.com/revit)


# Description

This sample demonstrates a way to draw curves through DA4R

# Development Setup

## Prerequisites

1. **Forge Account**: Learn how to create a Forge Account, activate subscription and create an app at [this tutorial](http://learnforge.autodesk.io/#/account/). 
2. **Visual Studio 2022** (Windows).
3. **Revit 2022**: required to compile changes into the plugin

## Design Automation Setup

### Activity example

```json
{
  "id": "DrawCurvesActivity",
  "commandLine": [
    "$(engine.path)\\\\revitcoreconsole.exe /i \"$(args[inputFile].path)\" /al \"$(appbundles[DrawCurves].path)\""
  ],
  "parameters": {
    "inputFile": {
      "verb": "get",
      "description": "Input Revit File",
      "required": true,
      "localName": "$(inputFile)"
    },
    "inputJson": {
      "zip": false,
	  "verb": "get",
	  "description": "Input json",
	  "required": true,
	  "localName": "params.json"
    },
    "result": {
      "zip": false,
      "verb": "put",
      "description": "Revit model with added curves",
      "localName": "OutputFile.rvt"
    }
  },
  "engine": "Autodesk.Revit+2022",
  "appbundles": [
    "Autodesk.DrawCurves+dev"
  ],
  "description": "Draw curves on Revit file using Design Automation"
}
```

### Workitem example

```json
{
  "activityId": "ID OF THE ACTIVITY",
  "arguments": {
    "inputFile": {
      "url": "URL TO DOWNLOAD THE INPUT FILE (BIM360/ACC)",
      "Headers": {
        "Authorization": "Bearer TOKEN"
      }
    },
	"inputJson": {
	  "verb": "get",
	  "url": "data:application/json,{
		  "ViewName": "Nível 1",
		  "Points": [
			{
			  "X": 2.62655557052325,
			  "Y": 47.8415984391073,
			  "Z": 0
			},
			{
			  "X": -26.8978097037929,
			  "Y": 12.6558300543774,
			  "Z": 0
			},
			{
			  "X": 3.26142034026135,
			  "Y": -12.6507687521793,
			  "Z": 0
			},
			{
			  "X": 51.7263688152716,
			  "Y": -4.1050907173895,
			  "Z": 0
			}
		  ]
	  }"
	},
    "result": {
      "verb": "post",
      "url": "URL TO UPLOAD THE RESULT",
      "Headers": {
        "Content-Type": "application/json"
      }
    },
    "onProgress": {
      "verb": "post",
      "url": "URL TO RECEIVE PROGRESS INFORMATION",
      "Headers": {
        "Content-Type": "application/json"
      }
    },
    "onComplete": {
      "verb": "post",
      "url": "URL TO RECEIVE STATUS AFTER JOB EXECUTION",
      "Headers": {
        "Content-Type": "application/json"
      }
    }
  }
}
```

## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE) file for full details.

## Written by

João Martins [@JooPaulodeOrne2](http://twitter.com/JooPaulodeOrne2), [Forge Partner Development](http://forge.autodesk.com)