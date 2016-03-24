namespace Bsg.Ef6.Tests.TestCases
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using Context;
    using Data.Context;
    using Data.Domain;
    using Data.Repo;
    using Extensions;
    using NUnit.Framework;
    using TestInfrastructure;

    [TestFixture]
    public class TruncateTests : TestBase
    {
        #region Tests
        [Test]
        public void EnsureTruncateRemovesAllRecords()
        {
            // Arrange
            var requestContainer = this.BuildRequestContainer();
            this.CleanPrimarySchema(requestContainer);

            var betaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Beta>>();
            var alphaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Alpha>>();
            var contextSession = requestContainer.GetService<IDbContextSession<PrimaryContext>>();

            var alpha = new Alpha
            {
                IsActive = true,
                Name = "Some Alpha"
            };

            alphaPrimaryRepo.AddOne(alpha);
            Assert.IsTrue(contextSession.HasChanges());
            contextSession.CommitChanges();
            Assert.Greater(alpha.Id, 0);

            var betas = new List<Beta>();
            var noOfRecordsToInsert = 1000;

            for (var idx = 1; idx <= noOfRecordsToInsert; idx++)
            {
                betas.Add(new Beta
                {
                    Code = idx.ToString(),
                    AlphaId = alpha.Id
                });
            }

            betaPrimaryRepo.BulkAdd(betas);

            // Assume
            var betasBeforeTruncate = betaPrimaryRepo.CountAll();
            Assert.Greater(betasBeforeTruncate, 0);

            // Action
            using (var transaction = contextSession.StartNewTransaction())
            {
                betaPrimaryRepo.Truncate(transaction);
                transaction.Commit();
            }

            // Assert
            var betasAfterTruncate = betaPrimaryRepo.CountAll();
            Assert.AreEqual(betasAfterTruncate, 0);
        }

        [Test]
        public void EnsureTruncateReseedsIdFrom1AfterTruncate()
        {
            // Arrange
            var requestContainer = this.BuildRequestContainer();
            this.CleanPrimarySchema(requestContainer);

            var betaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Beta>>();
            var alphaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Alpha>>();
            var contextSession = requestContainer.GetService<IDbContextSession<PrimaryContext>>();

            var alpha = new Alpha
            {
                IsActive = true,
                Name = "Some Alpha"
            };

            alphaPrimaryRepo.AddOne(alpha);
            Assert.IsTrue(contextSession.HasChanges());
            contextSession.CommitChanges();
            Assert.Greater(alpha.Id, 0);

            var betas = new List<Beta>();
            var noOfRecordsToInsert = 1000;

            for (var idx = 1; idx <= noOfRecordsToInsert; idx++)
            {
                betas.Add(new Beta
                {
                    Code = idx.ToString(),
                    AlphaId = alpha.Id
                });
            }

            betaPrimaryRepo.BulkAdd(betas);

            // Assume
            var betasBeforeTruncate = betaPrimaryRepo.CountAll();
            Assert.Greater(betasBeforeTruncate, 0);

            // Action
            using (var transaction = contextSession.StartNewTransaction())
            {
                betaPrimaryRepo.Truncate(transaction);
                transaction.Commit();
            }

            betas = new List<Beta>
            {
                new Beta
                {
                    Code = "XYZ",
                    AlphaId = alpha.Id
                }
            };

            betaPrimaryRepo.BulkAdd(betas);

            // Assert
            var betasAfterTruncateAndOneAdd = betaPrimaryRepo.CountAll();
            var onlyBetaPrimaryRepoId = betaPrimaryRepo.FindOne().Id;

            Assert.AreEqual(betasAfterTruncateAndOneAdd, 1);
            Assert.AreEqual(onlyBetaPrimaryRepoId, 1);
        }

        [Test]
        public void EnsureTruncateWithForeignKeyConstraintsRemovesAllRecords()
        {
            // Arrange
            var requestContainer = this.BuildRequestContainer();
            this.CleanPrimarySchema(requestContainer);

            var alphaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Alpha>>();
            var contextSession = requestContainer.GetService<IDbContextSession<PrimaryContext>>();

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
            var alphasBeforeTruncate = alphaPrimaryRepo.CountAll();
            Assert.Greater(alphasBeforeTruncate, 0);

            // Action
            using (var transaction = contextSession.StartNewTransaction())
            {
                alphaPrimaryRepo.TruncateWithForeignKeys(transaction);
                transaction.Commit();
            }

            // Assert
            var alphasAfterTruncate = alphaPrimaryRepo.CountAll(e => e.IsActive);
            Assert.AreEqual(alphasAfterTruncate, 0);
        }

        [Test]
        public void EnsureTruncateWithForeignKeyConstraintsReseedsIdFrom1AfterTruncate()
        {
            // Arrange
            var requestContainer = this.BuildRequestContainer();
            this.CleanPrimarySchema(requestContainer);

            var alphaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Alpha>>();
            var contextSession = requestContainer.GetService<IDbContextSession<PrimaryContext>>();

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

            // Action
            using (var transaction = contextSession.StartNewTransaction())
            {
                alphaPrimaryRepo.TruncateWithForeignKeys(transaction);
                transaction.Commit();
            }

            alphas = new List<Alpha>
            {
                new Alpha
                {
                    Name = "After Truncate",
                    IsActive = true
                }
            };

            alphaPrimaryRepo.BulkAdd(alphas);

            // Assert
            var alphasAfterTruncateAndOneAdd = alphaPrimaryRepo.CountAll();
            var onlyAlphaId = alphaPrimaryRepo.FindOne().Id;

            Assert.AreEqual(alphasAfterTruncateAndOneAdd, 1);
            Assert.AreEqual(onlyAlphaId, 1);
        }

        [Test]
        public void TruncateWillThrowSqlExceptionWhenTableHasForeignKeyConstraints()
        {
            // Arrange
            var requestContainer = this.BuildRequestContainer();
            this.CleanPrimarySchema(requestContainer);

            var alphaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Alpha>>();
            var contextSession = requestContainer.GetService<IDbContextSession<PrimaryContext>>();

            // Action
            var result = this.ActionWithException<SqlException>(() =>
            {
                using (var transaction = contextSession.StartNewTransaction())
                {
                    alphaPrimaryRepo.Truncate(transaction);
                }
            });

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.Message, "Cannot truncate table 'dbo.Alpha' because it is being referenced by a FOREIGN KEY constraint.");
        }
        #endregion

        #region Private Methods
        private void CleanPrimarySchema(IServiceProvider requestContainer)
        {
            var alphaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Alpha>>();
            var betaPrimaryRepo = requestContainer.GetService<IPrimaryRepository<Beta>>();
            var contextSession = requestContainer.GetService<IDbContextSession<PrimaryContext>>();

            using (var transaction = contextSession.StartNewTransaction())
            {
                betaPrimaryRepo.Truncate(transaction);
                alphaPrimaryRepo.TruncateWithForeignKeys(transaction);
                transaction.Commit();
            }
        } 
        #endregion
    }
}
