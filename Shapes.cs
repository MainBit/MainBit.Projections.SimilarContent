using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;
using Orchard.Forms.Services;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Projections.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MainBit.Projections.SimilarContent
{
    public class Shapes : IShapeTableProvider
    {
        private readonly Work<IContentManager> _contentManager;
        private readonly Work<IProjectionManager> _projectionManager;
        public Shapes(Work<IContentManager> contentManager,
            Work<IProjectionManager> projectionManager)
        {
            _contentManager = contentManager;
            _projectionManager = projectionManager;
		}
 
		public void Discover(ShapeTableBuilder builder) {
			builder.Describe("SimilarContentQueryPicker").OnDisplaying(context => {
                //var layouts = _projectionManager.Value.DescribeLayouts().SelectMany(x => x.Descriptors).ToList();
                //var queryRecordEntries = _contentManager.Value.Query("Query").ForPart<QueryPart>()
                //    .Join<TitlePartRecord>().OrderBy(x => x.Title).List()
                //                         .Select(x => new QueryRecordEntry
                //                         {
                //                             Id = x.Id,
                //                             Name = x.Name,
                //                             LayoutRecordEntries = x.Layouts.Select(l => new LayoutRecordEntry
                //                             {
                //                                 Id = l.Id,
                //                                 Description = GetLayoutDescription(layouts, l)
                //                             })
                //                         });

                //context.Shape.QueryRecordEntries = queryRecordEntries;

                var queryParts = _contentManager.Value.Query("Query").ForPart<QueryPart>()
                   .Join<TitlePartRecord>().OrderBy(x => x.Title).List();
                context.Shape.QueryParts = queryParts;
			});
		}

        private static string GetLayoutDescription(IEnumerable<LayoutDescriptor> layouts, LayoutRecord l)
        {
            var descriptor = layouts.FirstOrDefault(x => l.Category == x.Category && l.Type == x.Type);
            return String.IsNullOrWhiteSpace(l.Description) ? descriptor.Display(new LayoutContext { State = FormParametersHelper.ToDynamic(l.State) }).Text : l.Description;
        }
    }
}