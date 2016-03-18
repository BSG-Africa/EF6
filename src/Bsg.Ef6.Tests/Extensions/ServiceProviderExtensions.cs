namespace Bsg.Ef6.Tests.Extensions
{
    using System;

    public static class ServiceProviderExtensions
    {
        public static TService GetService<TService>(this IServiceProvider serviceProvider)
            where TService : class
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            return (TService)serviceProvider.GetService(typeof(TService));
        }

        public static IServiceProvider CreateScopedContainer(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            return serviceProvider.GetService<IServiceProvider>();
        }
    }
}