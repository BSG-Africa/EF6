namespace Bsg.Ef6.Context
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.MappingViews;

    public class Ef6DbMappingViewFactory : DbMappingViewCacheFactory
    {
        private readonly Ef6DbMappingViewCache viewCache;

        public Ef6DbMappingViewFactory(IDictionary<EntitySetBase, DbMappingView> generatedViews, string contextHash)
        {
            this.viewCache = new Ef6DbMappingViewCache(generatedViews, contextHash);
        }

        public override DbMappingViewCache Create(string conceptualModelContainerName, string storeModelContainerName)
        {
            return this.viewCache;
        }
    }
}