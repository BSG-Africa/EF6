namespace Bsg.Ef6.Repo
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Objects;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Dynamic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using Context;
    using Domain;
    using Mapping;
    using Timeout;

    public abstract class BulkEnabledRepository<TEntity, TContext> : Repository<TEntity, TContext>, IBulkEnabledRepository<TEntity, TContext>
        where TEntity : class, IEntity<TContext>, new()
        where TContext : IDbContext
    {
        private readonly IDbContextSession<TContext> session;

        private readonly ITimeoutCacheService timeoutCacheService;

        private readonly ITableMappingsCacheService tableMappingsCacheService;

        protected BulkEnabledRepository(
            IDbContextSession<TContext> session,
            ITimeoutCacheService timeoutCacheService,
            ITableMappingsCacheService tableMappingsCacheService)
            : base(session)
        {
            this.tableMappingsCacheService = tableMappingsCacheService;
            this.timeoutCacheService = timeoutCacheService;
            this.session = session;
        }

        #region Interface Methods
        public int BulkDelete(Expression<Func<TEntity, bool>> predicate)
        {
            return this.BulkDelete(this.FindAll(predicate));
        }

        public int BulkDelete(Expression<Func<TEntity, bool>> predicate, IContextTransaction contextTransaction)
        {
            return this.BulkDelete(this.FindAll(predicate), contextTransaction);
        }

        public int BulkDelete(IQueryable<TEntity> target)
        {
            return this.BulkDelete(target, null);
        }

        public int BulkDelete(
            IQueryable<TEntity> target, 
            IContextTransaction contextTransaction)
        {
            const string wrapperAlias = "a";
            const string originalAlias = "b";
            var tableName = this.GetMapping().FullyQualifiedTableName;
            var queryWithPrimaryKeySelected = this.BuildSelectExtractPrimaryKeys(target); 
            var joinWrapperAndOriginal = this.BuildWrapperJoinOnPrimaryKeys(wrapperAlias, originalAlias);

            // Delete using inner join on SELECT statement, which returns primary key collections of records to be removed 
            // More effecient than using WHERE PK1 in (SELECT PK1 from ....) type syntax, esp. when hundreds of records
            var deleteQuery =
                $"DELETE {wrapperAlias} FROM {tableName} AS {wrapperAlias} INNER JOIN ({queryWithPrimaryKeySelected.Item1}) AS {originalAlias} ON {joinWrapperAndOriginal}";

            return this.Execute(deleteQuery, queryWithPrimaryKeySelected.Item2, contextTransaction); 
        }

        public int Truncate()
        {
            return this.Truncate(null);
        }

        public int Truncate(IContextTransaction contextTransaction)
        {
            var tableName = this.GetMapping().FullyQualifiedTableName;

            var truncateQuery = $"TRUNCATE TABLE {tableName}";

            return this.Execute(truncateQuery, null, contextTransaction);
        }

        public int TruncateWithForeignKeys()
        {
            return this.TruncateWithForeignKeys(null);
        }

        public int TruncateWithForeignKeys(IContextTransaction contextTransaction)
        {
            var tableName = this.GetMapping().FullyQualifiedTableName;

            // Adapted from http://stackoverflow.com/questions/472578/dbcc-checkident-sets-identity-to-0 
            // Issue when previously no records and reseed to 0, will start ids at 0 instead of 1
            // Changing RESEED to 1, could potentially start ids at 2
            // Logic below checks if the Id column has already been seeded and if so then reseeds to 0, which will result in the next Id of 1
            // if Id column has not been used then the RESEED is not required, as the next Id will be 1 anyway
            var truncateQuery = $"DELETE FROM {tableName}; IF EXISTS (SELECT * FROM sys.identity_columns WHERE QUOTENAME(OBJECT_SCHEMA_NAME(object_id)) + '.' + QUOTENAME(OBJECT_NAME(object_id)) = '{tableName}' AND last_value IS NOT NULL) DBCC CHECKIDENT ('{tableName}',RESEED, 0);";

            return this.Execute(truncateQuery, null, contextTransaction);
        }

        public int BulkUpdate(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TEntity>> updateExpression)
        {
            return this.BulkUpdate(this.FindAll(predicate), updateExpression);
        }

        public int BulkUpdate(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TEntity>> updateExpression, IContextTransaction contextTransaction)
        {
            return this.BulkUpdate(this.FindAll(predicate), updateExpression, contextTransaction);
        }

        public int BulkUpdate(IQueryable<TEntity> target, Expression<Func<TEntity, TEntity>> updateExpression)
        {
            return this.BulkUpdate(target, updateExpression, null);
        }

        public int BulkUpdate(
            IQueryable<TEntity> target,
            Expression<Func<TEntity, TEntity>> updateExpression,
            IContextTransaction contextTransaction)
        {
            const string wrapperAlias = "a";
            const string originalAlias = "b";
            var tableName = this.GetMapping().FullyQualifiedTableName;
            var setQuery = this.BuildSetQuery(updateExpression, wrapperAlias);
            var selectQuery = this.BuildSelectExtractPrimaryKeys(target);
            var joinWrapperAndOriginal = this.BuildWrapperJoinOnPrimaryKeys(wrapperAlias, originalAlias);

            var allParameters = setQuery.Item2.ToList().Union(selectQuery.Item2.ToList());

            // Update using inner join on SELECT statement,  which returns primary key collections of records to be updated
            // More effecient than using WHERE PK1 in (SELECT PK1 from ....) syntax
            var updateQuery =
                $"UPDATE {wrapperAlias} SET {setQuery.Item1} FROM {tableName} AS {wrapperAlias} INNER JOIN ({selectQuery.Item1}) AS {originalAlias} ON {joinWrapperAndOriginal}";

            return this.Execute(updateQuery, allParameters, contextTransaction);
        }

        public int BulkSelectAndUpdate(IQueryable query)
        {
            return this.BulkSelectAndUpdate(query, null);
        }

        public int BulkSelectAndUpdate(IQueryable query, IContextTransaction contextTransaction)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var selectBindings = this.ExtractSelectBindings(query);
            var mappings = this.GetMapping();
            var selectQuery = this.BuildSelect(query);
            var selectParameters = selectQuery.Item2;
            var requiredSelectLines = this.FixAdditionalSelectedColumn(selectQuery.Item1, selectBindings.Count);
            var actualSelectQuery = string.Join(Environment.NewLine, requiredSelectLines);

            // gets alias using Regex which might be a bit brittle,
            // possibily find a way to get alias's from IQueryable
            var columnAliasMappings = this.MapColumnsToAliases(selectBindings, mappings, requiredSelectLines);

            const string fromAlias = "a";
            var tableName = mappings.FullyQualifiedTableName;
            var setQuery = this.BuildComplexUpdateSetQuery(mappings, fromAlias, columnAliasMappings);
            var whereTableKeyEqualsSelectKey = this.BuildComplexUpdateWhere(mappings, fromAlias, columnAliasMappings);

            var updateQuery =
                $"UPDATE {tableName} SET {setQuery} FROM ({actualSelectQuery}) AS {fromAlias} WHERE {whereTableKeyEqualsSelectKey}";

            return this.Execute(updateQuery, selectParameters, contextTransaction); 
        }
        #endregion

        #region Protected Methods

        protected TableMapping<TEntity, TContext> GetMapping()
        {
            return this.GetMapping<TEntity>();
        } 

        protected int Execute(
           string sql,
           IEnumerable<ObjectParameter> parameters,
           IContextTransaction contextTransaction)
        {
            if (contextTransaction == null)
            {
                if (this.session.HasCurrentTransaction())
                {
                    return this.ExecuteWithExternalTransaction(sql, parameters, this.session.CurrentTransaction());
                }

                return this.ExecuteWithLocalTransaction(sql, parameters);
            }

            return this.ExecuteWithExternalTransaction(sql, parameters, contextTransaction);
        }

        protected int ExecuteWithLocalTransaction(
            string sql,
            IEnumerable<ObjectParameter> parameters)
        {
            using (var contextTransaction = this.session.StartNewTransaction())
            {
                try
                {
                    var result = this.ExecuteWithExternalTransaction(sql, parameters, contextTransaction);
                    contextTransaction.Commit();
                    return result;
                }
                catch (Exception)
                {
                    contextTransaction?.Rollback();
                    throw;
                }
            }
        }

        protected int ExecuteWithExternalTransaction(
            string sql,
            IEnumerable<ObjectParameter> parameters,
            IContextTransaction contextTransaction)
        {
            if (contextTransaction == null)
            {
                throw new ArgumentNullException(nameof(contextTransaction));
            }

            var sqlTransaction = contextTransaction.UnderlyingTransaction<SqlTransaction>();

            if (sqlTransaction == null)
            {
                throw new InvalidOperationException("Bulk Inserter requires a SQL Transaction");
            }

            var sqlConnection = sqlTransaction.Connection;

            return this.ExecuteBulkNonQuery(sql, parameters, sqlTransaction, sqlConnection);
        }

        protected Tuple<string, IEnumerable<ObjectParameter>> BuildSelect(IQueryable source)
        {
            var objectQuery = this.ExtractObjectQueryFromIQueryable(source);
            return new Tuple<string, IEnumerable<ObjectParameter>>(objectQuery.ToTraceString(), objectQuery.Parameters);
        }

        // Entity Framework sometimes adds an extra select item to the query. Thus removing if present
        protected IList<string> FixAdditionalSelectedColumn(string fullSelect, int expectedBindingsCount)
        {
            if (string.IsNullOrWhiteSpace(fullSelect))
            {
                throw new ArgumentNullException(nameof(fullSelect));
            }

            var selectLines = fullSelect.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            var actualProperties = 0;

            for (var i = 1; i < selectLines.Count; i++)
            {
                if (selectLines[i].TrimStart().StartsWith("FROM", StringComparison.Ordinal))
                {
                    break;
                }

                actualProperties++;
            }

            if (actualProperties > expectedBindingsCount)
            {
                // remove the 2nd line
                return selectLines.Take(1).Concat(selectLines.Skip(2)).ToList();
            }

            return selectLines;
        }

        protected ReadOnlyCollection<MemberBinding> ExtractSelectBindings(IQueryable query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var methodCallExpression = (MethodCallExpression)query.Expression;
            var selectArgument = ((UnaryExpression)methodCallExpression.Arguments[methodCallExpression.Arguments.Count - 1]).Operand;
            var selectLambdaExpressionBody = ((LambdaExpression)selectArgument).Body;
            return ((MemberInitExpression)selectLambdaExpressionBody).Bindings;
        }

        #endregion

        #region Private Methods

        private Tuple<string, IEnumerable<ObjectParameter>> BuildSelectExtractPrimaryKeys(IQueryable source)
        {
            var primaryKeyProjection = this.BuildDynamicProjectionUsingPrimaryKeys();
            var primaryQuerySelect = source.Select(primaryKeyProjection);
            return this.BuildSelect(primaryQuerySelect);
        }

        private string BuildDynamicProjectionUsingPrimaryKeys()
        {
            var tableMapping = this.GetMapping();

            if (!tableMapping.HasPrimaryKeys)
            {
                throw new InvalidOperationException(
                    $"Table {tableMapping.FullyQualifiedTableName} has no Primary Keys, unable to perform bulk operation");
            }

            var dynamicSelectProjection = new StringBuilder("new (");
            var isFirst = true;

            foreach (var primaryKey in tableMapping.PrimaryKeys)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    dynamicSelectProjection.Append(", ");
                }

                dynamicSelectProjection.Append(primaryKey.Key);
            }

            dynamicSelectProjection.Append(")");
            return dynamicSelectProjection.ToString();
        }

        private string BuildWrapperJoinOnPrimaryKeys(string wrapperAlias, string originalAlias)
        {
            var tableMapping = this.GetMapping();

            if (!tableMapping.HasPrimaryKeys)
            {
                throw new InvalidOperationException(
                    $"Table {tableMapping.FullyQualifiedTableName} has no Primary Keys, unable to perform bulk operation");
            }

            var join = new StringBuilder();
            var isFirst = true;

            foreach (var primaryKey in tableMapping.PrimaryKeys)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    join.Append(" AND ");
                }

                join.Append(string.Format("{0}.[{2}] = {1}.[{2}]", wrapperAlias, originalAlias, primaryKey.Value));
            }

            return join.ToString();
        }

        private Tuple<string, IEnumerable<ObjectParameter>> BuildSetQuery(
            Expression<Func<TEntity, TEntity>> updateExpression, 
            string targetPrefix)
        {
            var setQuery = new StringBuilder();
            var parameterCollection = new List<ObjectParameter>();
            var memberInitExpression = (MemberInitExpression)updateExpression.Body;
            var fieldIdx = -1;
            var mappings = this.GetMapping();

            foreach (var memberBinding in memberInitExpression.Bindings)
            {
                var memberAssignment = (MemberAssignment)memberBinding;
                var memberExpression = memberAssignment.Expression;

                fieldIdx++;

                if (fieldIdx > 0)
                {
                    setQuery.Append(", ");
                }

                var propertyName = memberBinding.Member.Name;
                var columnName = mappings.ColumnMappings[propertyName];

                object value;

                if (memberExpression.NodeType == ExpressionType.Constant)
                {
                    var constantExpression = (ConstantExpression)memberExpression;
                    value = constantExpression.Value;
                }
                else
                {
                    var lambda = Expression.Lambda(memberExpression, null);
                    value = lambda.Compile().DynamicInvoke();
                }

                if (value != null)
                {
                    var parameterName = "p__update__" + fieldIdx;
                    setQuery.AppendFormat("{0}.[{1}] = @{2}", targetPrefix, columnName, parameterName);
                    parameterCollection.Add(new ObjectParameter(parameterName, value));
                }
                else
                {
                    setQuery.AppendFormat("{0}.[{1}] = NULL", targetPrefix, columnName);
                }
            }

            return new Tuple<string, IEnumerable<ObjectParameter>>(setQuery.ToString(), parameterCollection);
        }

        private IDictionary<string, string> MapColumnsToAliases(
            IReadOnlyList<MemberBinding> selectBindings,
            TableMapping<TEntity, TContext> mappings,
            IList<string> selectLines)
        {
            var noOfBindings = selectBindings.Count;
            var aliasRegex = new Regex(@".+ AS \[(?<alias>[A-Za-z0-9_]+)\]$");
            var aliasMappings = new Dictionary<string, string>();

            if (selectLines == null
                || selectLines.Count < noOfBindings + 2
                || !selectLines[0].Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
                || !selectLines[noOfBindings + 1].Trim().StartsWith("FROM", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Unexpected SQL generated in Select And Update");
            }

            for (var idx = 0; idx < noOfBindings; idx++)
            {
                var selectLine = selectLines[idx + 1].Trim().TrimEnd(',');

                if (!aliasRegex.IsMatch(selectLine))
                {
                    throw new InvalidOperationException("Unexpected SQL generated in Select And Update");
                }

                var alias = aliasRegex.Match(selectLine).Groups["alias"].Value;
                var binding = selectBindings[idx];
                var propertyName = binding.Member.Name;
                var columnName = mappings.ColumnMappings[propertyName];

                aliasMappings.Add(columnName, alias);
            }

            return aliasMappings;
        }

        private string BuildComplexUpdateWhere(TableMapping<TEntity, TContext> tableMapping, string fromAlias, IDictionary<string, string> columnAliasMapping)
        {
            var sb = new StringBuilder();

            var tableName = tableMapping.FullyQualifiedTableName;
            var firstColumn = true;

            foreach (var columnName in tableMapping.PrimaryKeys.Values)
            {
                if (!firstColumn)
                {
                    sb.Append(" AND ");
                }

                if (!columnAliasMapping.ContainsKey(columnName))
                {
                    throw new InvalidOperationException($"Primary Key {columnName} is missing from Select And Update query for table: {tableName}");
                }

                var columnAlias = columnAliasMapping[columnName];

                sb.Append($"{tableName}.[{columnName}] = {fromAlias}.[{columnAlias}]");
                firstColumn = false;
            }

            return sb.ToString();
        }

        private object BuildComplexUpdateSetQuery(TableMapping<TEntity, TContext> mappings, string fromAlias, IDictionary<string, string> columnAliasMappings)
        {
            var sb = new StringBuilder();
            var tableName = mappings.FullyQualifiedTableName;
            var validColumns = mappings.ColumnMappings.Values;
            var primaryKeyColumns = mappings.PrimaryKeys.Values;
            var isFirstColumn = true;
            var hasAtLeastOneSet = false;

            foreach (var columnName in columnAliasMappings.Keys)
            {
                // Only add if not primary key
                if (!primaryKeyColumns.Contains(columnName))
                {
                    if (!validColumns.Contains(columnName))
                    {
                        throw new InvalidOperationException($"Column: {columnName} does not exist in table: {tableName}");
                    }

                    var alias = columnAliasMappings[columnName];

                    if (!isFirstColumn)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append($"[{columnName}] = {fromAlias}.[{alias}]");
                    hasAtLeastOneSet = true;

                    isFirstColumn = false;
                }
            }

            sb.AppendLine();

            if (!hasAtLeastOneSet)
            {
                throw new InvalidOperationException("No columns to Set in Select And Update");
            }

            return sb.ToString();
        }

        private TableMapping<TClass, TContext> GetMapping<TClass>() 
            where TClass : class, IEntity<TContext>, new()
        {
            var mappings = this.tableMappingsCacheService.RetrieveMappings<TContext>();
            return mappings.GetMapping<TClass, TContext>();
        }

        private ObjectQuery ExtractObjectQueryFromIQueryable(IQueryable query)
        {
            // first try direct cast
            var objectQuery = query as ObjectQuery;
            if (objectQuery != null)
            {
                return objectQuery;
            }

            try
            {
                var internalQuery = this.GetValueFromProperty(query, "InternalQuery");
                return (ObjectQuery)this.GetValueFromProperty(internalQuery, "ObjectQuery");
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Query that is not an Object Query cannot be used for bulk operations");
            }
        }

        private object GetValueFromProperty(object instance, string propertyName)
        {
            var property = instance.GetType()
                               .GetProperty(
                                   propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            return property.GetValue(instance, null);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private int ExecuteBulkNonQuery(
            string sql,
            IEnumerable<ObjectParameter> parameters,
            SqlTransaction sqlTransaction,
            SqlConnection sqlConnection)
        {
            var command = sqlConnection.CreateCommand();
            command.Transaction = sqlTransaction;
            command.CommandText = sql;
            command.CommandTimeout = this.timeoutCacheService.BulkUpdateTimeout<TContext>();

            if (parameters != null)
            {
                // parameters are used when strings are used in the source IQueryable
                foreach (var objectParameter in parameters)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = objectParameter.Name;
                    parameter.Value = objectParameter.Value;
                    command.Parameters.Add(parameter);
                }
            }
          
            return command.ExecuteNonQuery();
        }
        #endregion
    }
}
