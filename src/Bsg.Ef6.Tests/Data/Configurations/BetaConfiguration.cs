namespace Bsg.Ef6.Tests.Data.Configurations
{
    using System.Data.Entity.ModelConfiguration;
    using Domain;

    public class BetaConfiguration : EntityTypeConfiguration<Beta>
    {
        public BetaConfiguration()
        {
            this.Property(e => e.Code).HasMaxLength(10);
        }
    }
}
