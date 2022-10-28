# Revit ModelCurve Creator bundle for APS (formerly Forge) Design Automation
![Platforms](https://img.shields.io/badge/platform-Windows|MacOS-lightgray.svg)
![.NET](https://img.shields.io/badge/.NET%206-blue.svg)
[![License](http://img.shields.io/:license-MIT-blue.svg)](http://opensource.org/licenses/MIT)

[![oAuth2](https://img.shields.io/badge/oAuth2-v1-green.svg)](http://developer.autodesk.com/)
[![Data-Management](https://img.shields.io/badge/Data%20Management-v1-green.svg)](http://developer.autodesk.com/)
[![Viewer](https://img.shields.io/badge/Viewer-v6-green.svg)](http://developer.autodesk.com/)

![Intermediate](https://img.shields.io/badge/Level-Intermediate-blue.svg)

# Description

This sample shows a way to draw lines in a Revit file from 2D Views in Viewer by using [Edit2D Extension](https://forge.autodesk.com/en/docs/viewer/v7/developers_guide/advanced_options/edit2d-setup/).
This is a basic web app that leverages the [Simple Viewer Tutorial](https://forge-tutorials.autodesk.io/tutorials/simple-viewer/) with a few modifications in order to triger a DA4R workitem from a polyline drawn in Viewer scene.
To make this work, you need to draw the polilyne inside a viewport from a sheet view (and the viewport need to be available as a translated viewable from the model).
Before taking advantage of this sample, you need to create an appbundle with the content from the DrawPolyLine project in this repo (feel free to modify it's logic').

## [Demo Video](https://youtu.be/Hwf2kOwfy2I)

# Setup

## Prerequisites

1. **DrawPolyLine bundle and activity ready**: Follow the steps at DrawPolyLine project in order to publish the proper appbundle and activity. This sample takes advantage of that.
2. **Forge Account**: Learn how to create an APS (formerly Forge) Account, activate subscription and create an app at [this tutorial](http://learnforge.autodesk.io/#/account/). 
3. **Visual Studio**: Either Community (Windows) or Code (Windows, MacOS).
4. **.NET 6** basic knowledge with C#
5. **JavaScript** basic knowledge

## Running locally

Clone this project or download it. It's recommended to install [GitHub desktop](https://desktop.github.com/). To clone it via command line, use the following (**Terminal** on MacOSX/Linux, **Git Shell** on Windows):

    git clone https://github.com/Autodesk-Forge/forge-viewhubs



**Environment variables**

At the `appsettings.json`, find the env vars and add your Forge Client ID, Secret and callback URL. Also define the `ASPNETCORE_URLS` variable. The end result should be as shown below:

```json
"FORGE_CLIENT_ID": "HERE GOES YOUE CLIENT ID",
"FORGE_CLIENT_SECRET": "HERE GOES YOUR CLIENT SECRET",
"FORGE_BUCKET": "HERE GOES YOUR BUCKET KEY"
```

# Further Reading

Documentation:

- [Design Automation for Revit](https://forge.autodesk.com/en/docs/design-automation/v3/tutorials/revit/)
- [Data Management API](https://developer.autodesk.com/en/docs/data/v2/overview/)
- [Viewer](https://developer.autodesk.com/en/docs/viewer/v6)

Tutorials:

- [Simple Viewer](https://forge-tutorials.autodesk.io/tutorials/simple-viewer/)
- [Modify your models](https://learnforge.autodesk.io/#/tutorials/modifymodels)


### Tips & Tricks

This sample uses .NET 6 and works fine on both Windows and MacOS, see [this tutorial for MacOS](https://github.com/augustogoncalves/dotnetcoreheroku).
It's just recomended to use Visual Studio 2022 for Windows, so you're able to compile both appbundle and web app.
Follow the steps under DrawPolyLine project to create the bundle and activity under your account.
**This only works on sheets**, so you'll need to perform the edit2d steps in a Revit 2D sheet.
Also, please note that **the viewport from sheet needs to be translated for this to work!**.

### Troubleshooting

1. **Cannot trigger against a specific view**: Ensure your view have been translated, [learn more here](https://knowledge.autodesk.com/support/revit/learn-explore/caas/CloudHelp/cloudhelp/2021/ENU/Revit-Cloud/files/GUID-09FBF9E2-6ECF-447D-8FA8-12AB16495BC3-htm.html).

2. **error setting certificate verify locations** error: may happen on Windows, use the following: `git config --global http.sslverify "false"`

## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE) file for full details.

## Written by

João Martins [@JooPaulodeOrne2](http://twitter.com/JooPaulodeOrne2), [Forge Partner Development](http://forge.autodesk.com)