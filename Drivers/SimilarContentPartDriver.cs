using MainBit.Projections.SimilarContent.Models;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MainBit.Projections.SimilarContent.Settings;
using MainBit.Projections.SimilarContent.Services;

namespace MainBit.Projections.SimilarContent.Drivers
{
    public class SimilarContentPartDriver : ContentPartDriver<SimilarContentPart>
    {
        private readonly ISimilarContentService _similarContentService;
        public SimilarContentPartDriver(
            IShapeFactory shapeFactory,
            IOrchardServices services,
            ISimilarContentService similarContentService)
        {
            Services = services;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
            _similarContentService = similarContentService;
        }

        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        dynamic Shape { get; set; }

        protected override DriverResult Display(SimilarContentPart part, string displayType, dynamic shapeHelper)
        {
            if (displayType != "Detail") { return null; }

            var settings = part.TypePartDefinition.GetSimilarContentPartSettings();
            var contentItems = _similarContentService.GetSimilarContentItems(part);

            return Combined(
                ContentShape("Parts_Projections_SimilarContent", () =>
                {
                    var list = Services.New.List();
                    var contentShapes = contentItems.Select(item => Services.ContentManager.BuildDisplay(
                        item,
                        string.IsNullOrWhiteSpace(settings.DisplayType) ? "Summary" : settings.DisplayType));
                    list.AddRange(contentShapes);
                    return list;
                })
            );
        }
    }
}