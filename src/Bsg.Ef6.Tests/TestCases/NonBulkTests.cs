namespace Bsg.Ef6.Tests.TestCases
{
    using System.Collections.Generic;
    using System.Linq;
    using Data.Domain;
    using Data.Repo;
    using Extensions;
    using NUnit.Framework;
    using TestInfrastructure;

    [TestFixture]
    public class NonBulkTests : TestBase
    {
        #region Tests
        [Test]
        public void EnsureCountAllReturnsCorrectResult()
        {
            // Arrange
            var requestContainer = this.BuildRequestContainer();
            var alphaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Alpha>>();

            var alphas = new List<Alpha>();
            var noOfRecordsToInsert = 100;

            for (var idx = 1; idx <= noOfRecordsToInsert; idx++)
            {
                alphas.Add(new Alpha
                {
                    Name = idx.ToString(),
                    IsActive = idx % 2 == 0
                });
            }

            alphaPrimaryRepo.BulkAdd(alphas);

            // Action
            var activeAlphasDbCount = alphaPrimaryRepo.CountAll(e => e.IsActive);

            // Assert
            var activeAlphasMemoryCount = alphaPrimaryRepo.FindAll(e => e.IsActive).ToList().Count;
            Assert.Greater(activeAlphasDbCount, 0);
            Assert.AreEqual(activeAlphasDbCount, activeAlphasMemoryCount);
        } 
        #endregion
    }
}
