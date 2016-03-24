namespace Bsg.Ef6.Tests.Data.Configurations
{
    using System.Data.Entity.ModelConfiguration;

    public abstract class SchemaScopedConfiguration<TType> : EntityTypeConfiguration<TType>
        where TType : class
    {
        protected SchemaScopedConfiguration(string schemaName)
        {
            this.ToTable($"{schemaName}.{typeof(TType).Name}");
        }
    }
}