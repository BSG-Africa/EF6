namespace Bsg.Ef6.Tests.Data.Domain
{
    using Context;
    using Ef6.Domain;

    public class Beta : IEntity<PrimaryContext>
    {
        public virtual int Id { get; set; }
        
        public virtual string Code { get; set; }

        public virtual int AlphaId { get; set; }

        public virtual Alpha Alpha { get; set; }
    }
}
