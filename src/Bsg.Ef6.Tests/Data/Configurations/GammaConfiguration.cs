namespace Bsg.Ef6.Tests.Data.Configurations
{
    using System.Data.Entity.ModelConfiguration;
    using Domain;

    public class GammaConfiguration : EntityTypeConfiguration<Gamma>
    {
        public GammaConfiguration()
        {
            this.Property(e => e.Category).HasMaxLength(20);
            this.Property(e => e.Cost).HasPrecision(19, 2);
            this.Property(e => e.Price).HasPrecision(19, 2);
        }
    }
}
