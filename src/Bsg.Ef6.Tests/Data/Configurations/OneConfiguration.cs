namespace Bsg.Ef6.Tests.Data.Configurations
{
    using System.Data.Entity.ModelConfiguration;
    using Domain;

    public class OneConfiguration : EntityTypeConfiguration<One>
    {
        public OneConfiguration()
        {
            this.Property(e => e.Name).HasMaxLength(50);
        }
    }
}
