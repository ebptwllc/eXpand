﻿using System;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Win.Core.ModelEditor;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.Xpo;
using Xpand.ExpressApp.ModelDifference.Core;
using Xpand.ExpressApp.ModelDifference.DataStore.BaseObjects;
using System.Linq;

namespace Xpand.ExpressApp.ModelDifference.Win.PropertyEditors
{
    [PropertyEditor(typeof(ModelApplicationBase))]
    public class ModelEditorPropertyEditor : WinPropertyEditor, IComplexPropertyEditor
    {
        #region Members

        
        private ModelEditorViewController _controller;
        ModelApplicationBuilder _modelApplicationBuilder;
        ModelApplicationBase _masterModel;
        ModelApplicationBase _currentObjectModel;
        ObjectSpace _objectSpace;
        #endregion

        #region Constructor

        public ModelEditorPropertyEditor(Type objectType, IModelMemberViewItem model)
            : base(objectType, model)
        {
        }

        #endregion

        #region Properties

        public new ModelDifferenceObject CurrentObject
        {
            get { return base.CurrentObject as ModelDifferenceObject; }
            set { base.CurrentObject = value; }
        }

        public new ModelEditorControl Control
        {
            get { return (ModelEditorControl)base.Control; }
        }


        public ModelEditorViewController ModelEditorViewController
        {
            get { return _controller ?? (_controller = GetModelEditorController()); }
        }


        #endregion

        #region Overrides

        protected override void ReadValueCore()
        {
            base.ReadValueCore();
            if (_controller == null) {
                _controller = GetModelEditorController();
            }
        }


        protected override void OnCurrentObjectChanged(){
            if (Control != null){
                ResetModel();
            }
            var xafApplication = XpandModuleBase.Application;
            xafApplication.ViewShowing+=ApplicationOnViewShowing;
            xafApplication.ViewShown+=XafApplicationOnViewShown;
            _modelApplicationBuilder = new ModelApplicationBuilder(CurrentObject.PersistentApplication.ExecutableName);
            _masterModel = _modelApplicationBuilder.GetMasterModel();
            base.OnCurrentObjectChanged();
        }

        void XafApplicationOnViewShown(object sender, ViewShownEventArgs viewShownEventArgs) {
            
        }

        void ApplicationOnViewShowing(object sender, ViewShowingEventArgs viewShowingEventArgs) {
            
        }


        protected override object CreateControlCore() {
            View.Closing+=ViewOnClosing;
            CurrentObject.Changed+=CurrentObjectOnChanged;
            _objectSpace.ObjectSaving+=ObjectSpaceOnObjectSaving;
            var modelEditorControl = new ModelEditorControl(new SettingsStorageOnDictionary());
            return modelEditorControl;
        }

        void CurrentObjectOnChanged(object sender, ObjectChangeEventArgs objectChangeEventArgs) {
            if (objectChangeEventArgs.PropertyName=="XmlContent") {
                ResetModel();
                _masterModel = _modelApplicationBuilder.GetMasterModel();
                _controller = GetModelEditorController();
            }
        }

        void ObjectSpaceOnObjectSaving(object sender, ObjectManipulatingEventArgs args) {
            if (ReferenceEquals(args.Object, CurrentObject)){
                new ModelValidator().ValidateModel(_currentObjectModel);
                ModelEditorViewController.SaveAction.Active["Not needed"] = true;
                ModelEditorViewController.Save();
                CurrentObject.UpdateAspects(_currentObjectModel);
                ModelEditorViewController.SaveAction.Active["Not needed"] = false;
            }
        }

        void ViewOnClosing(object sender, EventArgs eventArgs) {
            _objectSpace.ObjectSaving-=ObjectSpaceOnObjectSaving;
            ResetModel();
        }

        void ResetModel() {
            _modelApplicationBuilder.ResetModel(_masterModel);
        }
        #endregion

        #region Eventhandler

        private void Model_Modifying(object sender, CancelEventArgs e)
        {
            View.ObjectSpace.SetModified(CurrentObject);
        }

#endregion

        #region Methods

        public void Setup(ObjectSpace objectSpace, XafApplication application)
        {
            _objectSpace = objectSpace;
        }


        private ModelEditorViewController GetModelEditorController()
        {
            var allLayers = CurrentObject.GetAllLayers(_masterModel);
            _currentObjectModel = allLayers.Where(@base => @base.Id==CurrentObject.Name).Single();
            _masterModel.AddLayers(allLayers);
            var controller = new ModelEditorViewController((IModelApplication) _masterModel, null, null);
            controller.SetControl(Control);
            controller.Modifying += Model_Modifying;
            controller.SaveAction.Active["Not needed"] = false;

            return controller;
        }

        #endregion
    }
}