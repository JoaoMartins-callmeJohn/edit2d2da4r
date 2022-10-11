class DrawCurvesToolExtension extends Autodesk.Viewing.Extension {
  constructor(viewer, options) {
    super(viewer, options);
    //this.tool = new DrawCurvesTool();
  }

  async load() {
    const edit2d = await this.viewer.loadExtension('Autodesk.Edit2D');
    // Register all standard tools in default configuration
    edit2d.registerDefaultTools();
    const ctx = edit2d.defaultContext;
    // {EditLayer} Edit layer containing your shapes
    ctx.layer
    // {EditLayer} An additional layer used by tools to display temporary shapes (e.g. dashed lines for snapping etc.)
    ctx.gizmoLayer
    // {UndoStack} Manages all modifications and tracks undo/redo history
    ctx.undoStack
    // {Selection} Controls selection and hovering highlight
    ctx.selection
    // {Edit2DSnapper} Edit2D snapper
    ctx.snapper
    ctx.undoStack.addEventListener(Autodesk.Edit2D.UndoStack.AFTER_ACTION, this.afterAction.bind(this));
    return true;
  }

  getPointsFromAction(edit2DPoints, viewport) {
    let points = [];
    //Here we get the viewport from first selected point
    edit2DPoints.map(point => points.push(this.sheetToWorld(point, this.viewer.model, viewport)));

    return points;
  }

  sheetToWorld(sheetPos, model2d, viewport) {
    const sheetUnitScale = model2d.getUnitScale();
    //const globalOffset = model3d.getData().globalOffset;
    const matrix = viewport.get2DTo3DMatrix(sheetUnitScale);
    const worldPos = new THREE.Vector3(sheetPos.x, sheetPos.y, matrix.elements[14]).applyMatrix4(matrix);
    return worldPos;
  }

  getViewport(edit2DPoints, viewer) {
    const viewportExt = viewer.getExtension('Autodesk.AEC.ViewportsExtension');
    return viewportExt.findViewportAtPoint(viewer.model, new THREE.Vector2(edit2DPoints[0].x, edit2DPoints[0].y));
  }

  getViewPortName(viewer, viewGuid) {
    let viewName = null;
    viewer.model.getDocumentNode().parent.parent.children.forEach(folder => {
      try {
        folder.data.children.forEach(viewType => {
          viewType.children.forEach(view => {
            if (view.guid == viewGuid) {
              viewName = view.name;
            }
          })
        });
      }
      catch (error) {
        //in this case, the bubble doesn't contain children
      }
    });
    return viewName;

  }

  afterAction(event) {
    let data = {};
    let viewport = this.getViewport(event.action.shape._loops[0], this.viewer);
    data.points = this.getPointsFromAction(event.action.shape._loops[0], viewport);
    data.urn = atob(this.viewer.model.getSeedUrn());
    data.viewname = this.getViewPortName(this.viewer, viewport.viewportRaw.viewGuid);

    if (data.points.includes(null)) {
      Swal.fire({
        title: 'Error!',
        icon: 'error',
        text: 'Not able to transform all selected points!',
        showConfirmButton: false,
        timer: 5000
      });
    }
    else {
      Swal.fire({
        title: 'Add drawn curve to the model!',
        showCancelButton: true,
        confirmButtonText: 'Submit workitem',
        showLoaderOnConfirm: true,
        preConfirm: () => {
          return fetch(`/api/DA4R/workitem`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
          })
            .then(response => {
              if (!response.ok) {
                throw new Error(response.statusText)
              }
              return response.json();
            })
            .catch(error => {
              Swal.showValidationMessage(
                `Request failed: ${error}`
              )
            })
        },
        allowOutsideClick: () => !Swal.isLoading()
      }).then((result) => {
        if (result.isConfirmed) {
          Swal.fire({
            icon: 'success',
            title: `Here's your workitem id: ${result.value.workItemId}!`,
            showConfirmButton: false
          });
          this.poolWorkItem(result.value.workItemId, result.value.uploadKey, result.value.bucketKey, result.value.objectName);
        }
      })
    }
  }

  poolWorkItem(workitemId, uploadKey, bucketKey, objectName) {
    setTimeout(async () => {
      let response = await fetch(`/api/DA4R/workitem?workitemId=${workitemId}&uploadkey=${uploadKey}&bucketKey=${bucketKey}&objectName=${objectName}`);
      let jsonResponse = await response.json();
      if (jsonResponse.status == 'Inprogress') {
        Swal.fire({
          toast: true,
          position: 'top',
          timer: 3000,
          text: `Status: ${jsonResponse.status}`,
          showConfirmButton: false
        });
        this.poolWorkItem(workitemId, uploadKey, bucketKey, objectName);
      }
      else {
        Swal.fire({
          toast: true,
          position: 'top',
          timer: 7000,
          html: `Report URL <a href=${jsonResponse.reportUrl}>here</a>`,
          icon: jsonResponse.status == 'success' ? 'success' : 'error',
          showConfirmButton: false
        });
      }
        
    }, 4000);
  }

  unload() {
    return true;
  }

  // Convenience function for tool switching per console. E.g. startTool(tools.polygonTool)
  startTool(tool) {

    var controller = this.viewer.toolController;

    // Check if currently active tool is from Edit2D
    var activeTool = controller.getActiveTool();
    var isEdit2D = activeTool && activeTool.getName().startsWith("Edit2");

    // deactivate any previous edit2d tool
    if (isEdit2D) {
      controller.deactivateTool(activeTool.getName());
      activeTool = null;
    }

    // stop editing tools
    if (!tool) {
      return;
    }

    controller.activateTool(tool.getName());
  }

  endTool(tool) {
    var controller = this.viewer.toolController;
    controller.deactivateTool(tool.getName());
  }

  onToolbarCreated(toolbar) {
    this.button = new Autodesk.Viewing.UI.Button('curves-draw-tool-button');
    this.button.onClick = (ev) => {
      if (true) {
        const edit2d = this.viewer.getExtension('Autodesk.Edit2D');
        const layer = edit2d.defaultContext.layer;
        const tools = edit2d.defaultTools;
        if (this.button.getState() == Autodesk.Viewing.UI.Button.State.INACTIVE) {
          this.button.setState(Autodesk.Viewing.UI.Button.State.ACTIVE);
          this.startTool(tools.polylineTool);
        }
        else {
          this.button.setState(Autodesk.Viewing.UI.Button.State.INACTIVE);
          this.endTool(tools.polylineTool);
        }
      }
    };

    this.group = new Autodesk.Viewing.UI.ControlGroup('draw-tool-group');
    this.group.addControl(this.button);
    toolbar.addControl(this.group);
  }
}

Autodesk.Viewing.theExtensionManager.registerExtension('Edit2D2DA4RExtension', DrawCurvesToolExtension);