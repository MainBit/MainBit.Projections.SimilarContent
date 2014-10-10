using Orchard.ContentManagement;
using Orchard.Core.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MainBit.Projections.SimilarContent.Models
{
    public class SimilarContentPart : ContentPart
    {
        private static readonly char[] separator = new[] { '{', '}', ',' };

        public int[] Ids
        {
            get { return DecodeIds(Retrieve<string>("Ids")); }
            set { Store<string>("Ids", EncodeIds(value)); }

        }

        private string EncodeIds(ICollection<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return string.Empty;
            }

            // use {1},{2} format so it can be filtered with delimiters
            return "{" + string.Join("},{", ids.ToArray()) + "}";
        }

        private int[] DecodeIds(string ids)
        {
            if (String.IsNullOrWhiteSpace(ids))
            {
                return new int[0];
            }

            return ids.Split(separator, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
        }
    }
}