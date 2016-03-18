namespace Bsg.Ef6.Tests.Data.Domain
{
    using Context;
    using Ef6.Domain;

    public class Two : IEntity<SecondaryContext>
    {
        public virtual int Id { get; set; }

        public virtual string Code { get; set; }

        public virtual int OneId { get; set; }

        public virtual One One { get; set; }
    }
}
