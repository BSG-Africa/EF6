namespace Bsg.Ef6.Tests.Data.Configurations
{
    using Domain;

    public class BetaConfiguration : SchemaScopedConfiguration<Beta>
    {
        public BetaConfiguration()
            : base("NotDbo")
        {
            this.Property(e => e.Code).HasMaxLength(10);
        }
    }
}
