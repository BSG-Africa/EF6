namespace Bsg.Ef6.Timeout
{
    using System;
    using System.Reflection;

    public interface ITimeoutService
    {
        void BuildAndCacheAllTimeouts(Assembly assemblyWithContexts);

        void BuildAndCacheAllTimeouts(params Type[] types);
    }
}