namespace Bsg.Ef6.Tests
{
    using System.Collections.Generic;
    using Data.Context;
    using Data.Domain;
    using NUnit.Framework;
    using Extensions;
    using Repo;

    [TestFixture]
    public class PlaceholderTests1 : TestBase
    {
        [Test]
        public void PlaceholderTest()
        {
            var requestContainer = this.BuildRequestContainer();

            var alphaRepo = requestContainer.GetService<IGenericRepository<Alpha, PrimaryContext>>();
            var oneRepo = requestContainer.GetService<IGenericRepository<One, SecondaryContext>>();

            var alphas = new List<Alpha>();

            for (var idx = 1; idx <= 10000; idx++)
            {
                alphas.Add(new Alpha
                {
                    Name = idx.ToString()
                });
            }

            alphaRepo.BulkAdd(alphas);

            Assert.AreEqual(1, 1);
        }
    }
}
