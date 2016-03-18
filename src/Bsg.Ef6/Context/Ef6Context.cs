namespace Bsg.Ef6.Context
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Dynamic;
    using System.Reflection;
    using Domain;
    using Mapping;

    /// <summary>
    /// Don't need Strongly Typed DbSets e.g. public DbSet<Type/> DefaultTypes { get; set; }
    /// as the various configurations will setup the model relationships
    /// and the Session and Repository are generic
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public abstract class Ef6Context : DbContext, IDbContext
    {
        #region Constructors

        /// <summary>
        /// Using DbConnection contructor to support different provider types
        /// </summary>
        protected Ef6Context(DbConnection dbConnection, bool contextOwnsConnection)
            : base(dbConnection, contextOwnsConnection)
        {
        }

        #endregion Constructors

        #region Interface Methods

        public bool IsDirty()
        {
            return this.ChangeTracker.HasChanges();
        }

        public IDbSet<TEntity> EntitySet<TEntity, TContext>()
            where TEntity : class, IEntity<TContext>, new()
            where TContext : IDbContext
        {
            return this.Set<TEntity>();
        }

        public void SetLogger(Action<string> logAction)
        {
            this.Database.Log = logAction;
        }

        public void SetCommandTimeout(int timeout)
        {
            // Get the ObjectContext related to this DbContext
            var objectContext = this.GetObjectContext();
            objectContext.CommandTimeout = timeout;
        }

        /// <summary>
        /// This is done once per App Domain, and should not be repeated 
        /// </summary>
        public void BindPreGeneratedViews()
        {
            var errors = new List<EdmSchemaError>();
            var mapping = this.GetStorageMapping();

            var views = mapping.GenerateViews(errors);
            var contextHash = mapping.ComputeMappingHashValue();

            if (errors.Count > 0)
            {
                throw new InvalidOperationException("Generating context views resulted in errors.");
            }

            if (mapping.MappingViewCacheFactory == null)
            {
                mapping.MappingViewCacheFactory = new Ef6DbMappingViewFactory(views, contextHash);
            }

            // not strictly necessary (as have already generated the views) and might be a bit fragile
            // but will load cache at this point and save an additional few micro seconds on first request
            // potentially also a bit inefficient getting all types across all loaded assemblies
            // if this causes issues, these 3 lines can be safely removed
            var typeName = views.First().Key.EntityContainer.EntitySets.First().Name;
            var type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).First(x => x.Name == typeName);
            this.Set(type).Count();
        }

        public TableMapping<TEntity, TContext> GetMapping<TEntity, TContext>()
            where TEntity : class, IEntity<TContext>, new()
            where TContext : IDbContext
        {
            var storageMetadata = this.GetStoreCollection();
            var entityType = storageMetadata.GetItem<EntityType>($"CodeFirstDatabaseSchema.{typeof(TEntity).Name}");
            var mappings = new TableMapping<TEntity, TContext>();

            var primaryKeys = entityType.KeyMembers;

            foreach (var declaredProperty in entityType.DeclaredProperties)
            {
                var columnName = declaredProperty.Name;
                var propertyName =
                    declaredProperty.MetadataProperties.First(e => e.Name == "PreferredName").Value.ToString();
                mappings.ColumnMappings.Add(propertyName, columnName);
            }

            foreach (var primaryKey in primaryKeys)
            {
                var propertyName = primaryKey.MetadataProperties.First(e => e.Name == "PreferredName").Value.ToString();
                mappings.PrimaryKeys.Add(propertyName, primaryKey.Name);
            }

            var metadataProperty = entityType.MetadataProperties.FirstOrDefault(e => e.Name == "TableName");

            if (metadataProperty != null)
            {
                var tableName = this.GetValueFromProperty<string>(metadataProperty.Value, "Name");
                var schemaName = this.GetValueFromProperty<string>(metadataProperty.Value, "Schema");

                mappings.FullyQualifiedTableName = $"[{schemaName}].[{tableName}]";
            }
            else
            {
                mappings.FullyQualifiedTableName = $"[{entityType.Name}]";
            }

            return mappings;
        }

        public ContextTableMappings GetMappings<TContext>()
            where TContext : IDbContext
        {
            var actualContextType = this.GetType();
            var contextEntityType = this.GetEntityInterfaceType();
            var wrapperType = typeof(IWrapperEntity);

            var entityTypesToMap =
                this.GetTypesInContextImplementationAssembly()
                    .Where(t => contextEntityType.IsAssignableFrom(t) && !wrapperType.IsAssignableFrom(t))
                    .ToList();

            var mappings = new ContextTableMappings();

            var getMappingMethod = actualContextType.GetMethod("GetMapping");

            foreach (var entityType in entityTypesToMap)
            {
                var getMappingGenericMethod = getMappingMethod.MakeGenericMethod(entityType, actualContextType);
                dynamic entityTableMapping = getMappingGenericMethod.Invoke(this, null);
                
                var addMappingMethod = typeof(ContextTableMappings).GetMethod("AddMapping");
                var addMappingGenericMethod = addMappingMethod.MakeGenericMethod(entityType, actualContextType);

                addMappingGenericMethod.Invoke(mappings, new object[] { entityTableMapping });
            }

            return mappings;
        }

        #endregion Interface Methods

        #region Overrides

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            this.AddConventions(modelBuilder);
            this.AddConfigurations(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        protected abstract void AddConventions(DbModelBuilder modelBuilder);

        protected virtual void AddConfigurations(DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            var entityTypeConfigType = typeof(EntityTypeConfiguration<>);
            var contextEntityType = this.GetEntityInterfaceType();
            var types = this.GetTypesInContextImplementationAssembly();
            var wrapperType = typeof(IWrapperEntity);

            var entityConfigurationsForContext =
                types.Where(
                    t =>
                    t.IsClass && !t.IsAbstract && t.BaseType != null && t.BaseType.IsGenericType
                    && (t.BaseType.GetGenericTypeDefinition() == entityTypeConfigType)
                    && t.BaseType.GenericTypeArguments.Length > 0
                    && contextEntityType.IsAssignableFrom(t.BaseType.GenericTypeArguments[0])).ToList();

            foreach (var configurationType in entityConfigurationsForContext)
            {
                dynamic configurationInstance = Activator.CreateInstance(configurationType);
                modelBuilder.Configurations.Add(configurationInstance);
            }

            var wrapperEntities = types.Where(t => wrapperType.IsAssignableFrom(t) && contextEntityType.IsAssignableFrom(t)).ToList();
            modelBuilder.Ignore(wrapperEntities);
        }

        #endregion Overrides

        #region Private Methods

        private ObjectContext GetObjectContext()
        {
            return (this as IObjectContextAdapter).ObjectContext;
        }

        private StorageMappingItemCollection GetStorageMapping()
        {
            return
                (StorageMappingItemCollection)this.GetObjectContext().MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
        }

        private StoreItemCollection GetStoreCollection()
        {
            return
                (StoreItemCollection)this.GetObjectContext().MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
        }

        private TType GetValueFromProperty<TType>(object instance, string propertyName)
        {
            var property = instance.GetType()
                               .GetProperty(
                                   propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            return (TType)property.GetValue(instance, null);
        }

        private Type GetEntityInterfaceType()
        {
            var entityType = typeof(IEntity<>);
            var contextType = this.GetType();
            return entityType.MakeGenericType(contextType);
        }

        private IList<Type> GetTypesInContextImplementationAssembly()
        {
            return Assembly.GetAssembly(this.GetType()).GetTypes().ToList();
        }

        #endregion
    }
}
