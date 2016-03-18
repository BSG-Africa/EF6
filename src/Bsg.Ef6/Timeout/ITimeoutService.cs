namespace Bsg.Ef6.Timeout
{
    using System.Reflection;

    public interface ITimeoutService
    {
        void BuildAndCacheAllTimeouts(Assembly assemblyWithContexts);
    }
}