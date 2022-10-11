﻿/// import * as Autodesk from "@types/forge-viewer";

async function getAccessToken(callback) {
    try {
        const resp = await fetch('/api/auth/token');
        if (!resp.ok) {
            throw new Error(await resp.text());
        }
        const { access_token, expires_in } = await resp.json();
        callback(access_token, expires_in);
    } catch (err) {
        alert('Could not obtain access token. See the console for more details.');
        console.error(err);
    }
}

export function initViewer(container) {
    return new Promise(function (resolve, reject) {
        Autodesk.Viewing.Initializer({ getAccessToken }, function () {
            const config = {
              extensions: ['Autodesk.AEC.LevelsExtension', , 'Autodesk.AEC.ViewportsExtension', 'Autodesk.DocumentBrowser', 'Edit2D2DA4RExtension']
            };
            const viewer = new Autodesk.Viewing.GuiViewer3D(container, config);
            viewer.start();

            resolve(viewer);
        });
    });
}

export function loadModel(viewer, urn) {
    return new Promise(function (resolve, reject) {
      function onDocumentLoadSuccess(doc) {
          doc.downloadAecModelData();
          load3DModel();
          resolve(viewer.loadDocumentNode(doc, doc.getRoot().getDefaultGeometry()));
      }
      function onDocumentLoadFailure(code, message, errors) {
          reject({ code, message, errors });
      }
      viewer.setLightPreset(0);
      Autodesk.Viewing.Document.load('urn:' + urn, onDocumentLoadSuccess, onDocumentLoadFailure);
    });
}

async function load3DModel() {
  Autodesk.Viewing.Initializer({ getAccessToken }, function () {
    const config = {
      extensions: ['Autodesk.AEC.LevelsExtension']
    };
    var htmlDiv = document.getElementById('hidden');
    viewer = new Autodesk.Viewing.GuiViewer3D(htmlDiv, config);
    var startedCode = viewer.start();
  });
}