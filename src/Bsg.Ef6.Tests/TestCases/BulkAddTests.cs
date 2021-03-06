﻿namespace Bsg.Ef6.Tests.TestCases
{
    using System.Collections.Generic;
    using Data.Domain;
    using Data.Repo;
    using Extensions;
    using NUnit.Framework;
    using TestInfrastructure;

    public class BulkAddTests : TestBase
    {
        #region Tests
        [Test]
        public void EnsureBulkAddInsertsRecords()
        {
            // Arrange
            var requestContainer = this.BuildRequestContainer();
            var alphaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Alpha>>();

            var alphasBeforeInsert = alphaPrimaryRepo.CountAll();

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

            // Action
            alphaPrimaryRepo.BulkAdd(alphas);

            // Assert
            var alphasAfterDelete = alphaPrimaryRepo.CountAll();
            Assert.That(alphasAfterDelete, Is.EqualTo(alphasBeforeInsert + noOfRecordsToInsert));
        }
        #endregion
    }
}
