namespace Bsg.Ef6.Tests.TestCases
{
    using System.Collections.Generic;
    using Context;
    using Data.Context;
    using Data.Domain;
    using Data.Repo;
    using Extensions;
    using NUnit.Framework;
    using Repo;
    using Services;
    using TestInfrastructure;

    [TestFixture]
    public class RepoVariantTests : TestBase
    {
        #region Tests
        [Test]
        public void EnsureThatAll3VariantsOfReposQuerySameTableCorrectly()
        {
            // Arrange
            var requestContainer = this.BuildRequestContainer();
            var alphaGenericRepo = requestContainer.GetService<IGenericRepository<Alpha, PrimaryContext>>();
            var alphaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Alpha>>();
            var alphaRepo = requestContainer.GetService<IAlphaRepository>();

            // Assume
            var firstcount = alphaGenericRepo.CountAll();
            Assert.AreEqual(firstcount, alphaPrimaryRepo.CountAll());
            Assert.AreEqual(firstcount, alphaRepo.CountAll());

            // Action
            var alphas = new List<Alpha>();
            var noOfRecordsToInsert = 10000;

            for (var idx = 1; idx <= noOfRecordsToInsert; idx++)
            {
                alphas.Add(new Alpha
                {
                    Name = idx.ToString(),
                    IsActive = idx % 2 == 0
                });
            }

            alphaRepo.BulkAdd(alphas);

            // Assert
            var secondcount = alphaGenericRepo.CountAll();
            Assert.AreEqual(firstcount + noOfRecordsToInsert, secondcount);
            Assert.AreEqual(secondcount, alphaPrimaryRepo.CountAll());
            Assert.AreEqual(secondcount, alphaRepo.CountAll());
        }

        [Test]
        public void EnsureThatAll3VariantsOfReposTrackSameChanges()
        {
            // Arrange
            var requestContainer = this.BuildRequestContainer();
            var alphaGenericRepo = requestContainer.GetService<IGenericRepository<Alpha, PrimaryContext>>();
            var alphaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Alpha>>();
            var alphaRepo = requestContainer.GetService<IAlphaRepository>();
            var contextSession = requestContainer.GetService<IDbContextSession<PrimaryContext>>();

            var alpha = new Alpha
            {
                IsActive = true,
                Name = "Some Alpha"
            };

            alphaPrimaryRepo.AddOne(alpha);
            contextSession.CommitChanges();

            var genericTracked = alphaGenericRepo.FindOneTracked(e => e.Id == alpha.Id);

            // Assume
            Assert.Greater(alpha.Id, 0);

            // Action
            var changed = "Changed";
            genericTracked.Name = changed;

            // Assert
            var primaryTracked = alphaPrimaryRepo.FindOneTracked(e => e.Id == alpha.Id);
            var alphaTracked = alphaRepo.FindOneTracked(e => e.Id == alpha.Id);
            Assert.AreEqual(primaryTracked.Name, changed);
            Assert.AreEqual(alphaTracked.Name, changed);
        } 
        #endregion
    }
}
