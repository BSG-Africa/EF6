namespace Bsg.Ef6.Context
{
    using System;
    using System.Reflection;

    public interface IContextService
    {
        void PreGenerateAllContextViews(Assembly assemblyWithContexts);

        void PreGenerateAllContextViews(params Type[] types);
    }
}