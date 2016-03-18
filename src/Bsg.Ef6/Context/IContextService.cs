namespace Bsg.Ef6.Context
{
    using System.Reflection;

    public interface IContextService
    {
        void PreGenerateAllContextViews(Assembly assemblyWithContexts);
    }
}