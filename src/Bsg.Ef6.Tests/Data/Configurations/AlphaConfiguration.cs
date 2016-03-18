namespace Bsg.Ef6.Tests.Data.Configurations
{
    using System.Data.Entity.ModelConfiguration;
    using Domain;

    public class AlphaConfiguration : EntityTypeConfiguration<Alpha>
    {
        public AlphaConfiguration()
        {
            this.Property(e => e.Name).HasMaxLength(50);
        }
    }
}
