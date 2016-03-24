namespace Bsg.Ef6.Tests.Data.Configurations
{
    using System.Data.Entity.ModelConfiguration;
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
