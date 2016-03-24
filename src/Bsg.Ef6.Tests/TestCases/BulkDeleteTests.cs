namespace Bsg.Ef6.Tests.TestCases
{
    using System.Collections.Generic;
    using Data.Domain;
    using Data.Repo;
    using Extensions;
    using NUnit.Framework;
    using TestInfrastructure;

    [TestFixture]
    public class BulkDeleteTests : TestBase
    {
        #region Tests
        [Test]
        public void EnsureBulkDeleteRemovesRecordsUsingIQueryable()
        {
            // Arrange
            var requestContainer = this.BuildRequestContainer();
            var alphaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Alpha>>();

            var alphas = new List<Alpha>();
            var noOfRecordsToInsert = 1000;

            for (var idx = 1; idx <= noOfRecordsToInsert; idx++)
            {
                alphas.Add(new Alpha
                {
                    Name = idx.ToString(),
                    IsActive = idx % 2 == 0
                });
            }

            alphaPrimaryRepo.BulkAdd(alphas);

            // Assume
            var activeAlphasBeforeDelete = alphaPrimaryRepo.CountAll(e => e.IsActive);
            Assert.Greater(activeAlphasBeforeDelete, 0);

            var alphasToDeleteQuery = alphaPrimaryRepo.FindAll(e => e.IsActive);

            // Action
            var recordsDeleted = alphaPrimaryRepo.BulkDelete(alphasToDeleteQuery);

            // Assert
            var activeAlphasAfterDelete = alphaPrimaryRepo.CountAll(e => e.IsActive);
            Assert.AreEqual(activeAlphasAfterDelete, 0);
            Assert.AreEqual(recordsDeleted, activeAlphasBeforeDelete);
        }

        [Test]
        public void EnsureBulkDeleteRemovesRecordsUsingExpression()
        {
            // Arrange
            var requestContainer = this.BuildRequestContainer();
            var alphaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Alpha>>();

            var alphas = new List<Alpha>();
            var noOfRecordsToInsert = 1000;

            for (var idx = 1; idx <= noOfRecordsToInsert; idx++)
            {
                alphas.Add(new Alpha
                {
                    Name = idx.ToString(),
                    IsActive = idx % 2 == 0
                });
            }

            alphaPrimaryRepo.BulkAdd(alphas);

            // Assume
            var activeAlphasBeforeDelete = alphaPrimaryRepo.CountAll(e => e.IsActive);
            Assert.Greater(activeAlphasBeforeDelete, 0);

            // Action
            var recordsDeleted = alphaPrimaryRepo.BulkDelete(e => e.IsActive);

            // Assert
            var activeAlphasAfterDelete = alphaPrimaryRepo.CountAll(e => e.IsActive);
            Assert.AreEqual(activeAlphasAfterDelete, 0);
            Assert.AreEqual(recordsDeleted, activeAlphasBeforeDelete);
        } 
        #endregion
    }
}
