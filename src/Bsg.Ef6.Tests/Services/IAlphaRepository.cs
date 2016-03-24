namespace Bsg.Ef6.Tests.Services
{
    using System.Linq;
    using Data.Domain;
    using Data.Repo;

    public interface IAlphaRepository : IPrimaryRepository<Alpha>
    {
        IQueryable<Alpha> Active();
    }
}