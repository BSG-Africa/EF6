namespace Bsg.Ef6.Tests.Data.Context
{
    using System;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using Ef6.Context;

    public class SecondaryContext : Ef6Context
    {
        public SecondaryContext(DbConnection dbConnection, bool contextOwnsConnection)
            : base(dbConnection, contextOwnsConnection)
        {
        }

        protected override void AddConventions(DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            // makes all properties required as the default
            // need to specify !IsGenericType to prevent nullables from been marked as non nullable
            modelBuilder.Properties().Where(p => !p.PropertyType.IsGenericType).Configure(c => c.IsRequired());

            // dafaults strings to not unicode
            modelBuilder.Properties<string>().Configure(c => c.IsUnicode(false));
        }
    }
}
