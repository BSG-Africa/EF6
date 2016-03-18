namespace Bsg.Ef6.Tests.Data.Domain
{
    using Context;
    using Ef6.Domain;

    public class One : IEntity<SecondaryContext>
    {
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }
    }
}
