namespace Bsg.Ef6.Domain
{
    using System.Diagnostics.CodeAnalysis;
    using Context;

    // ReSharper disable once UnusedTypeParameter
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IEntity<TContext>
        where TContext : IDbContext
    {
    }
}