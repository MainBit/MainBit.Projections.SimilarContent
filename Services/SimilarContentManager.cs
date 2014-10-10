using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Forms.Services;
using Orchard.Projections.Descriptors;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Property;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Models;
using Orchard.Tokens;
using Orchard.Projections.Services;
using System.Reflection;
using MainBit.Projections.SimilarContent.FilterEditors.Forms;

namespace MainBit.Projections.SimilarContent.Services {
    public class SimilarContentManager : ISimilarContentManager {
        private readonly ITokenizer _tokenizer;
        private readonly IEnumerable<IFilterProvider> _filterProviders;
        private readonly IEnumerable<ISortCriterionProvider> _sortCriterionProviders;
        private readonly IEnumerable<ILayoutProvider> _layoutProviders;
        private readonly IEnumerable<IPropertyProvider> _propertyProviders;
        private readonly IContentManager _contentManager;
        private readonly IRepository<QueryPartRecord> _queryRepository;
        private readonly IProjectionManager _projectionManager;
        

        public SimilarContentManager(
            ITokenizer tokenizer,
            IEnumerable<IFilterProvider> filterProviders,
            IEnumerable<ISortCriterionProvider> sortCriterionProviders,
            IEnumerable<ILayoutProvider> layoutProviders,
            IEnumerable<IPropertyProvider> propertyProviders,
            IContentManager contentManager,
            IRepository<QueryPartRecord> queryRepository,
            IProjectionManager projectionManager)
        {
            _tokenizer = tokenizer;
            _filterProviders = filterProviders;
            _sortCriterionProviders = sortCriterionProviders;
            _layoutProviders = layoutProviders;
            _propertyProviders = propertyProviders;
            _contentManager = contentManager;
            _queryRepository = queryRepository;
            _projectionManager = projectionManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public int GetCount(int queryId, IDictionary<string, object> tokens = null)
        {

            var queryRecord = _queryRepository.Get(queryId);

            if (queryRecord == null) {
                throw new ArgumentException("queryId");
            }

            // aggregate the result for each group query

            return GetContentQuery(queryRecord, Enumerable.Empty<SortCriterionRecord>(), 0, tokens).Count();
        }

        public IEnumerable<ContentItem> GetContentItems(int queryId, int neededCount, IDictionary<string, object> tokens = null)
        {
            var queryRecord = _queryRepository.Get(queryId);

            if(queryRecord == null) {
                throw new ArgumentException("queryId");
            }

            var contentQuery = GetContentQuery(queryRecord, queryRecord.SortCriteria, neededCount, tokens);
            return contentQuery.List();
        }

        public IHqlQuery GetContentQuery(QueryPartRecord queryRecord, IEnumerable<SortCriterionRecord> sortCriterions, int neededCount, IDictionary<string, object> tokens = null)
        {
            var availableSortCriteria = _projectionManager.DescribeSortCriteria().ToList();
            var descriptorFilterStates = GetOptimumFilters(queryRecord, neededCount, tokens);

            var contentQuery = _contentManager.HqlQuery().ForVersion(VersionOptions.Published);

            // iterate over each filter to apply the alterations to the query object
            foreach (var descriptorFilterState in descriptorFilterStates) {

                var filterContext = new FilterContext
                {
                    Query = contentQuery,
                    State = descriptorFilterState.TokenizedState
                };
                descriptorFilterState.Descriptor.Filter(filterContext);
                contentQuery = filterContext.Query;
            }

            // iterate over each sort criteria to apply the alterations to the query object
            foreach (var sortCriterion in sortCriterions)
            {
                var sortCriterionContext = new SortCriterionContext {
                    Query = contentQuery,
                    State = FormParametersHelper.ToDynamic(sortCriterion.State)
                };

                string category = sortCriterion.Category;
                string type = sortCriterion.Type;

                // look for the specific filter component
                var descriptor = availableSortCriteria
                    .SelectMany(x => x.Descriptors)
                    .FirstOrDefault(x => x.Category == category && x.Type == type);

                // ignore unfound descriptors
                if (descriptor == null) {
                    continue;
                }

                // apply alteration
                descriptor.Sort(sortCriterionContext);

                contentQuery = sortCriterionContext.Query;
            }

            return contentQuery;
        }

        public List<DescriptorFilterState> GetOptimumFilters(QueryPartRecord queryRecord, int neededCount, IDictionary<string, object> tokens = null)
        {
            // process only one filter group
            if(queryRecord.FilterGroups.Count != 1) {
                throw new IndexOutOfRangeException();
            }

            var availableFilters = _projectionManager.DescribeFilters().ToList();

            tokens = tokens ?? new Dictionary<string, object>();
            var processedFilter = new Dictionary<int, bool>();
            var filters = queryRecord.FilterGroups.First().Filters;
            var descriptorFilterStates = filters.Select(f => new DescriptorFilterState
            { 
                Filter = f,
                Descriptor = availableFilters
                        .Where(td => td.Category == f.Category)
                        .SelectMany(td => td.Descriptors)
                        .FirstOrDefault(d => d.Type == f.Type),
                TokenizedState = FormParametersHelper.ToDynamic(_tokenizer.Replace(f.State, tokens))
            }).Where(p => p.Descriptor != null).ToList();

            while (processedFilter.Count < descriptorFilterStates.Count)
            {
                var contentQuery = _contentManager.HqlQuery().ForVersion(VersionOptions.Published);
                var currentDescriptorFilterStates = descriptorFilterStates
                    .Where(p => !processedFilter.ContainsKey(p.Filter.Id) || processedFilter[p.Filter.Id] == true);
                // iterate over each filter to apply the alterations to the query object
                foreach (var descriptorFilterState in currentDescriptorFilterStates)
                {
                    processedFilter[descriptorFilterState.Filter.Id] = false;
                    var filterContext = new FilterContext
                    {
                        Query = contentQuery,
                        State = descriptorFilterState.TokenizedState
                    };

                    try
                    {
                        descriptorFilterState.Descriptor.Filter(filterContext);
                    }
                    catch (NotNeedApplyFilterException ex)
                    {
                        continue;
                    }

                    var count = filterContext.Query.Count();
                    if (count < neededCount)
                    {
                        break;
                    }
                    processedFilter[descriptorFilterState.Filter.Id] = true;
                    contentQuery = filterContext.Query;
                }
            }

            var applyFilterIds = processedFilter.Where(p => p.Value).Select(p => p.Key);
            return descriptorFilterStates.Where(p => applyFilterIds.Contains(p.Filter.Id)).ToList();
        }
    }

    public class DescriptorFilterState
    {
        public FilterDescriptor Descriptor { get; set; }
        public FilterRecord Filter { get; set; }
        public dynamic TokenizedState { get; set; }
    }
}