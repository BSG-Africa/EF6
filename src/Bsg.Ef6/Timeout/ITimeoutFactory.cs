namespace Bsg.Ef6.Timeout
{
    using Context;

    public interface ITimeoutFactory
    {
        ContextTimeouts BuildTimeouts<TContext>()
            where TContext : IDbContext;
    }
}