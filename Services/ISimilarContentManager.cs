using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Projections.Descriptors;
using Orchard.Projections.Descriptors.Property;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard;
using System;

namespace MainBit.Projections.SimilarContent.Services
{
    public interface ISimilarContentManager : IDependency {

        IEnumerable<ContentItem> GetContentItems(int queryId, int neededCount, IDictionary<string, object> tokens = null);
        int GetCount(int queryId, IDictionary<string, object> tokens = null);
    }
}