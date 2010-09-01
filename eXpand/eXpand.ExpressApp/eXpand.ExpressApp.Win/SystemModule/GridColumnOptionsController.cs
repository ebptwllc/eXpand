﻿using System.Collections.Generic;
using DevExpress.ExpressApp.Model;
using DevExpress.XtraGrid.Columns;
using eXpand.ExpressApp.Core.DynamicModel;
using eXpand.ExpressApp.SystemModule;

namespace eXpand.ExpressApp.Win.SystemModule {
    public interface IModelColumnOptions : IModelColumnOptionsBase {
        IModelGridColumnOptions GridColumnOptions { get; set; }
    }

    public interface IModelGridColumnOptions : IModelNode {
        IModelGridColumnOptionsColumn OptionsColumn { get; set; }
        IModelGridColumnOptionsColumnFilter OptionsFilter { get; set; }
    }

    public interface IModelGridColumnOptionsColumn : IModelNode {
    }

    public interface IModelGridColumnOptionsColumnFilter : IModelNode {
    }

    public class GridColumnOptionsController : ColumnOptionsController
    {
        protected override IEnumerable<DynamicModelType> GetDynamicModelTypes() {
            yield return new DynamicModelType(typeof(IModelGridColumnOptionsColumn), typeof(OptionsColumn));
            yield return new DynamicModelType(typeof(IModelGridColumnOptionsColumnFilter), typeof(OptionsColumnFilter));
        }


    }
}