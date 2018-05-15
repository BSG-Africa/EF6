namespace Bsg.Ef6.Tests.TestInfrastructure
{
    using System;
    using System.Data.Entity;
    using Container;
    using Context;
    using Data.Context;
    using Data.Domain;
    using Data.Repo;
    using Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class TestBase
    {
        private IServiceProvider applicationContainer;

        [OneTimeSetUp]
        public void Setup()
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<PrimaryContext>());
            Database.SetInitializer(new DropCreateDatabaseAlways<SecondaryContext>());
            this.applicationContainer = new TestIocBootstrapper().BuildAutofacContainer();
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            var contextFactory = this.BuildRequestContainer().GetService<IDbContextFactory>();
            var primaryConext = contextFactory.BuildContext<PrimaryContext>();
            primaryConext.Database.Delete();

            var secondaryConext = contextFactory.BuildContext<SecondaryContext>();
            secondaryConext.Database.Delete();

            this.applicationContainer = null;
        }

        protected IServiceProvider BuildRequestContainer()
        {
            return this.applicationContainer.CreateScopedContainer();
        }

        protected TException ActionWithException<TException>(Action action) 
            where TException : Exception
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            try
            {
                action();
            }
            catch (TException exception)
            {
                return exception;
            }

            return null;
        }

        protected void CleanPrimarySchema(IServiceProvider requestContainer)
        {
            var alphaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Alpha>>();
            var betaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Beta>>();
            var gammaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Gamma>>();
            var contextSession = requestContainer.GetService<IDbContextSession<PrimaryContext>>();

            using (var transaction = contextSession.StartNewTransaction())
            {
                betaPrimaryRepo.Truncate(transaction);
                gammaPrimaryRepo.Truncate(transaction);
                alphaPrimaryRepo.TruncateWithForeignKeys(transaction);
                transaction.Commit();
            }
        }
    }
}
