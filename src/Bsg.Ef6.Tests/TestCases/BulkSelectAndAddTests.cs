namespace Bsg.Ef6.Tests.TestCases
{
    using System.Collections.Generic;
    using System.Linq;
    using Data.Domain;
    using Data.Repo;
    using Data.Wrappers;
    using Extensions;
    using NUnit.Framework;
    using TestInfrastructure;

    [TestFixture]
    public class BulkSelectAndAddTests : TestBase
    {
        #region Tests
        [Test]
        public void EnsureBulkSelectAndAddInsertsRecords()
        {
            // Arrange
            var requestContainer = this.BuildRequestContainer();
            this.CleanPrimarySchema(requestContainer);

            var alphaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Alpha>>();
            var gammaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Gamma>>();

            var gammas = new List<Gamma>();
            var noOfRecordsToInsert = 1000;

            for (var idx = 1; idx <= noOfRecordsToInsert; idx++)
            {
                var categoryName = idx % 10 == 0 ? "Category A" : idx % 3 == 0 ? "Category B" : "Other";

                gammas.Add(new Gamma
                {
                    Category = categoryName,
                    Cost = (idx * 1000m) - 100,
                    Price = (idx * 1000m) + idx
                });
            }

            gammaPrimaryRepo.BulkAdd(gammas);

            var insertAlphaQuery = gammaPrimaryRepo
                .FindAll(e => e.Category.StartsWith("Category"))
                .GroupBy(e => e.Category)
                .Select(g => new AlphaWrapper
                {
                    Name = g.Key,
                    IsActive = g.Sum(e => e.Price - e.Cost) > 100000m
                });

            // Assume
            var alphasBeforeInsert = alphaPrimaryRepo.CountAll();
            Assert.That(alphasBeforeInsert, Is.EqualTo(0));

            // Action
            alphaPrimaryRepo.BulkSelectAndAdd(insertAlphaQuery);

            // Assert
            var alphasAfterInsert = alphaPrimaryRepo.CountAll();
            Assert.That(alphasAfterInsert, Is.EqualTo(2));
        }
        #endregion
    }
}
