namespace Bsg.Ef6.Mapping
{
    using Context;

    public class TableMappingsFactory : ITableMappingsFactory
    {
        private readonly IDbContextFactory contextFactory;

        public TableMappingsFactory(IDbContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public ContextTableMappings BuildTableMappings<TContext>()
            where TContext : IDbContext
        {
            using (var context = this.contextFactory.BuildContext<TContext>())
            {
                return context.GetMappings<TContext>();
            }
        }
    }
}
