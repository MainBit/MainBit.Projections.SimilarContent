using Orchard.ContentManagement.MetaData.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MainBit.Projections.SimilarContent.Settings {
    public class SimilarContentPartSettings
    {
        public int Items { get; set; }
        public string DisplayType { get; set; }
        public int QueryPartRecord_Id { get; set; }
        //public int LayoutRecord_Id { get; set; }
    }

    public static class SimilarContentPartSettingsExtensions
    {
        public static SimilarContentPartSettings GetSimilarContentPartSettings(this ContentTypePartDefinition definition)
        {
            var settings = definition.Settings.GetModel<SimilarContentPartSettings>();
            return settings;
        }
    }

    //public class SimilarContentPartSettingsViewModel
    //{
    //    public int Items { get; set; }
    //    public string DisplayType { get; set; }
    //    public string QueryLayoutRecordId { get; set; }
    //}
}
