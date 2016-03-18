namespace Bsg.Ef6.Tests.Data.Configurations
{
    using System.Data.Entity.ModelConfiguration;
    using Domain;

    public class TwoConfiguration : EntityTypeConfiguration<Two>
    {
        public TwoConfiguration()
        {
            this.Property(e => e.Code).HasMaxLength(10);
        }
    }
}
