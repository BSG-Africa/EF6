namespace Bsg.Ef6.Tests.Data.Configurations
{
    using Domain;

    public class TwoConfiguration : SchemaScopedConfiguration<Two>
    {
        public TwoConfiguration()
            : base("NotDbo")
        {
            this.Property(e => e.Code).HasMaxLength(10);
        }
    }
}
