using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using System;

namespace MainBit.Projections.SimilarContent.Settings
{
    public class SimilarContentPartSettingsHooks : ContentDefinitionEditorEventsBase
    {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) 
        {
            if (definition.PartDefinition.Name != "SimilarContentPart")
                yield break;

            var model = definition.Settings.GetModel<SimilarContentPartSettings>();
            //var viewModel = new SimilarContentPartSettingsViewModel()
            //{
            //    DisplayType = model.DisplayType,
            //    Items = model.Items,
            //    QueryLayoutRecordId = "-1"
            //};

            // concatenated Query and Layout ids for the view
            //if (model.QueryPartRecord_Id > 0)
            //{
            //    viewModel.QueryLayoutRecordId = model.QueryPartRecord_Id + ";";
            //}

            //if (model.LayoutRecord_Id > 0)
            //{
            //    viewModel.QueryLayoutRecordId += model.LayoutRecord_Id.ToString();
            //}
            //else
            //{
            //    viewModel.QueryLayoutRecordId += "-1";
            //}

            yield return DefinitionTemplate(model); //yield return DefinitionTemplate(viewModel);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel)
        {
            if (builder.Name != "SimilarContentPart")
                yield break;

            //var viewModel = new SimilarContentPartSettingsViewModel();
            //updateModel.TryUpdateModel(viewModel, "SimilarContentPartSettingsViewModel", null, null);
            //var queryLayoutIds = viewModel.QueryLayoutRecordId.Split(new[] { ';' });
            //var model = new SimilarContentPartSettings()
            //{
            //    DisplayType = viewModel.DisplayType,
            //    Items = viewModel.Items,
            //    LayoutRecord_Id = Int32.Parse(queryLayoutIds[1]),
            //    QueryPartRecord_Id = Int32.Parse(queryLayoutIds[0])
            //};

            var model = new SimilarContentPartSettings();
            updateModel.TryUpdateModel(model, "SimilarContentPartSettings", null, null);

            builder.WithSetting("SimilarContentPartSettings.Items", model.Items.ToString());
            if (model.DisplayType != null)
            {
                builder.WithSetting("SimilarContentPartSettings.DisplayType", model.DisplayType.ToString());
            }
            builder.WithSetting("SimilarContentPartSettings.QueryPartRecord_Id", model.QueryPartRecord_Id.ToString());
            //builder.WithSetting("SimilarContentPartSettings.LayoutRecord_Id", model.LayoutRecord_Id.ToString());

            yield return DefinitionTemplate(model); //yield return DefinitionTemplate(viewModel);
        }
    }
}
