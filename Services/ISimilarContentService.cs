using MainBit.Projections.SimilarContent.Models;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MainBit.Projections.SimilarContent.Settings;
using Orchard;

namespace MainBit.Projections.SimilarContent.Services
{
    public interface ISimilarContentService : IDependency
    {
        List<ContentItem> GetSimilarContentItems(SimilarContentPart part);
        void ClearCache(IEnumerable<string> types = null);
    }
}