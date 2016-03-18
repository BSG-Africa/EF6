namespace Bsg.Ef6.Context
{
    public interface IDbContextFactory
    {
        IDbContext BuildContext<TContext>()
            where TContext : IDbContext;

        void BindViews<TContext>()
            where TContext : IDbContext;
    }
}