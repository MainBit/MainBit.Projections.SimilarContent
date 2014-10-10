using MainBit.Projections.SimilarContent.Models;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MainBit.Projections.SimilarContent.Settings;
using Orchard.ContentTypes.Services;
using Orchard;

namespace MainBit.Projections.SimilarContent.Services
{
    public class SimilarContentService : ISimilarContentService
    {
        private readonly IContentManager _contentManager;
        private readonly ISimilarContentManager _similarContentManager;
        private readonly IOrchardServices _orchardServices;

        public SimilarContentService(IContentManager сontentManager,
            ISimilarContentManager similarContentManager,
            IOrchardServices orchardServices)
        {
            _contentManager = сontentManager;
            _similarContentManager = similarContentManager;
            _orchardServices = orchardServices;
        }
        public List<ContentItem> GetSimilarContentItems(SimilarContentPart part) {

            var settings = part.TypePartDefinition.GetSimilarContentPartSettings();
            if (part.Ids.Any())
            {
                var randomIds = part.Ids.OrderBy(p => Guid.NewGuid()).Take(4).ToList();
                return _contentManager
                    .GetMany<ContentItem>(randomIds, VersionOptions.Latest, QueryHints.Empty)
                    .ToList();
            }

            var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
            var contentItems = _similarContentManager.GetContentItems(settings.QueryPartRecord_Id, settings.Items, tokens);
            part.Ids = contentItems.Select(p => p.Id).ToArray();

            return contentItems.Take(settings.Items).ToList();
        }
        public void ClearCache(IEnumerable<string> types = null)
        {
            if (types == null || !types.Any())
            {
                var _contentDefinitionService = _orchardServices.WorkContext.Resolve<IContentDefinitionService>();
                types = _contentDefinitionService.GetTypes()
                    .Where(t => t.Parts.Any(p => p.PartDefinition.Name == "SimilarContentPart"))
                    .Select(t => t.Name);
            }
            var similarContentParts = _contentManager.Query<SimilarContentPart>(types.ToArray()).List();
            foreach (var similarContentPart in similarContentParts)
            {
                if (similarContentPart.Ids.Any())
                {
                    similarContentPart.Ids = new int[0];
                }
            }
        }
    }
}