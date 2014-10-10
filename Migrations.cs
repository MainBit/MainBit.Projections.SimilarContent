using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Core.Navigation.Models;
using Orchard.Data.Migration;
using MainBit.Projections.SimilarContent.Models;

namespace MainBit.Search.SimilarContent
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            
            ContentDefinitionManager.AlterPartDefinition(typeof(SimilarContentPart).Name, part => part
                .Attachable()
                );

            return 1;
        }
    }
}