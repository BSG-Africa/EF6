namespace Bsg.Ef6.Timeout
{
    using System.Reflection;
    using Context;

    public interface ITimeoutService
    {
        void BuildAndCacheAllTimeouts(Assembly assemblyWithContexts);

        void BuildAndCacheAllTimeouts(params IDbContext[] contexts);
    }
}