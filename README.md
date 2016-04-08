# EF6

Bsg.EF6 is a .Net based framework, extending Microsoft's Entity Framework 6 (Code First) with a specific focus on bulk operations.
The framework is used internally by the .Net teams at [Business Systems Group](http://www.bsg.co.za/) when working with Entity Framework 6. 

### What is its purpose?
The aim of the framework is leverage off the DbContext's standard functionality to automatically generate raw sql from strongly-typed IQueryables
and then combine these parameterised SELECT SQL statements with custom SQL to perform bulk SQL operations (INSERTS, UPDATES, DELETES etc.) in one command, directly on the SQL server without requiring the DbConext to submit multiple requests from the Application Layer.

### What are the advantages to using it?
Using this framework can reduce the work load and memory requirements on the Application Server, reduce the number of (and frequeny of opening and closing) connections required to communicate with the Database Server and significantly reduce the network traffic between the two. Whilst at the same time still leveraging most of the usual benefits of a traditional ORM. 

### What does this actually mean?
It means you can run bulk update, insert and delete SQL statements directly on the Database server, but still make use of a ORM.
I.e. you get the performance of SQL and the usability of the ORM.

### How is this different from other Bulk EF frameworks?
For some parts, its very similar especially with regards to the BulkAdd, BulkDelete and BulkUpdate type functionality.
Its more unique value proposition comes from the SelectAndAdd and SelectAndUpdate methods, 
which allow for bulk (many) and unique (per row) updates and inserts without sending any additional data across the network (assuming all the data needed for the update or insert already resides within the Database) 

## BulkDelete & BulkUpdate

![BulkAdd](docs/bulkdeleteupdate.png)

#### Overview
*TODO*
#### When To Use
*TODO*
#### Example
*TODO*
#### Overloads
*TODO*

## BulkAdd

![BulkAdd](docs/bulkadd.png)

#### Overview
*TODO*
#### When To Use
*TODO*
#### Example
*TODO*
#### Overloads
*TODO*

## Truncate

![BulkAdd](docs/truncate.png)

#### Overview
*TODO*
#### When To Use
*TODO*
#### Example
*TODO*
#### Overloads
*TODO*

## BulkSelectAndAdd & BulkSelectAndUpdate

![BulkAdd](docs/bulkselectand.png)

#### Overview
*TODO*
#### When To Use
*TODO*
#### Example
*TODO*
#### Overloads
*TODO*

## Traditional Entity Framework

![BulkAdd](docs/traditional.png)

#### Overview
*TODO*
#### When To Use
*TODO*
#### Example
*TODO*
#### Overloads
*TODO*

## Getting Started
The topics below should give a brief introduction to the more important components of the framework.
Alternatively the Bsg.Ef6.Tests project can also be seen as a mini POC showing how to setup the IOC containers, how to create custom DbContexts, how to create domain entities, configurations, context repos, entities repos and most importantly how to use the various methods of the abstract bulk repos defined in the framework.

*Note: the that all the Contexts (Primary, Secondary), Entities (Alpha, Beta, One, Two etc.) and all their properties within the test project are completely arbitary ito their names.*

### DI & Built-In Services
The Bsg.EF6 framework includes a number of required Services, most of which can be overridden with custom implementations.
All the services have a default concrete implementation corresponding to the same name (excluding the Interface "I" prefix).
The services rely on the Constructor Injection convention for their DI implementations. 
Unless otherwise stated below, a transient lifecycle (or equivelent) is recommened.

See code the *TestIocBootstrapper* class in the Bsg.Ef6.Tests project, for an example of implementing the framework's services in an IOC (in this case Autofac). 

#### Table Mapping Services####
**ITableMappingsCacheService** - Stores the cached values for table mappings (various mappings between DB Columns and Poco properties) for each context. This must be populated at system startup prior to calling any repo methods. This ideally should be a singleton service i.e. Singleton lifecycle.  

**ITableMappingsFactory** - Builds a *ContextTableMappings* object for a specific context - used by the *ITableMappingService* below. Potentailly not required (see *ITableMappingService*).

**ITableMappingService** - Facade across the ITableMappingsCacheService and ITableMappingsFactory service to build and cache every discovered implementation of *Ef6Context*. This should be called manually after the IOC container has been setup.
Alternatively *ITableMappingsFactory* and *ITableMappingService* can be ignored as long as the *ITableMappingsCacheService* has been populated (for every Ef6Context) at startup via some other custom code.

#### Timeout Services####
**ITimeoutCacheService** - Stores the cached values for various timeouts for each context. This must be populated at system startup prior to calling any repo methods. This ideally should be a singleton service i.e. Singleton lifecycle.

**ITimeoutFactory** - Builds a *ContextTimeouts* object for a specific context - used by the *ITimeoutService* below. Potentailly not required (see *ITimeoutService*).

**ITimeoutService** - Facade across the *ITimeoutCacheService* and *ITimeoutCacheFactory* service to build and cache every found discovered implementation of *Ef6Context*. This should be called manually after the IOC container has been setup.
Alternatively *ITimeoutCacheFactory* and *ITimeoutService* can be ignored as long as the *ITimeoutCacheService* has been populated (for every Ef6Context) at startup via some other custom code.

#### Context Services####
**IDbContextFactory** - Builds DbContexts (and can also bind pre-generated EF Views at startup).

**IDbContextSession** - Wrapper around the instance of *Ef6Context* and thus controls its creation and destruction when required. There is generally 1 instance per *Ef6Context* implementation per request (or equivalent scope) to be shared amongst all Repository instances related to that *Ef6Conext* Scoped. Thus a Scoped lifecycle (or equivalent) is required.

**IContextService** - Facade across the IDbContextFactory in order to bind pre-generated EF Views for every *Ef6Context* implementation, in order to speed up the first actual query. This is usually called once at the application startup.

#### Repository Services####
**IGenericRepository** - Is a downstream repository which extends all of *IRepository*, *IBulkInsertRepository* and *IBulkEndbaleRepository* and thus includes all the Bulk and Non-Bulk repo functionality. 
It will be specific to a *Ef6Context* type and an *IEntity* type linked to that context.  
This would normally be regsitered as an open generic in order to prevent having to specifiy each *IEntity* type manually when resgistring types on the IOC container.
All Repositories (including the *IGenericRepository*) wrap around the scoped IDbContextSession which in turn manages the specific instance of Ef6Context.
Alternatively, context specific repos or even entity specific repos can be created and registered by extending off of the IGenericRepository and when doing so, *IGenericRepository* can be ignored.

#### Utility Services ####
**IDatabaseConnectionFactory** - Builds *SqlConnections*

**IConfigurationService** - Retrieves app settings and connection strings from app.config.

**IBulkInserterFactory** - Builds a *BulkInserter* for a specific *IEntity*


### Ef6Context
Extends off the standard *DbContext* and contains extended functionality to: 

- build the *TableMapping* for the specific context (discovered from within the *StoreItemCollection*)
- discover and register all *EntityTypeConfigurations* relating to itself
- expose the generic *IDbSet*, to be used by the various repos

### Repository Pattern
*TODO*
### Generic Repository
*TODO*
### Context Repository
*TODO*
### Entity Repository
*TODO* 
### IWrapperEntity
*TODO*
### EntityTypeConfiguration
*TODO*
### Transactions
*TODO*
### app.config
*TODO*

## Limitations
### Code First
*TODO*
### SelectAndX : Projections using IWrapperEntity
*TODO*
### SelectAndX : Projections and order of properties
*TODO*
### Primary Keys
*TODO*
### Collections
*TODO*
### SqlServer
*TODO*
### Parallel Operations in same container scope
*TODO*
### Nuget Package
*TODO*

