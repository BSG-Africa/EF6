namespace Bsg.Ef6.Context
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.MappingViews;

    public class Ef6DbMappingViewCache : DbMappingViewCache
    {
        private readonly IDictionary<EntitySetBase, DbMappingView> generatedViews;

        public Ef6DbMappingViewCache(IDictionary<EntitySetBase, DbMappingView> generatedViews, string contextHash)
        {
            this.MappingHashValue = contextHash;
            this.generatedViews = generatedViews;
        }

        public override string MappingHashValue { get; }

        public override DbMappingView GetView(EntitySetBase extent)
        {
            if (this.generatedViews.ContainsKey(extent))
            {
                return this.generatedViews[extent];
            }

            return null;
        }
    }
}