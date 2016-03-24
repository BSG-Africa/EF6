namespace Bsg.Ef6.Tests.Services
{
    using System.Linq;
    using Data.Context;
    using Data.Domain;
    using Repo;

    public interface IOneRepository : IBulkInsertRepository<One, SecondaryContext>
    {
        IQueryable<One> AllJohns();
    }
}