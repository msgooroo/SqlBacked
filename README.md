SqlBacked
=========

A light weight .Net tool for generating classes from tables within an SQL database, complete with the basic CRUD operations,
(Get / Select, Insert, Update, Delete).  

SqlBacked also supports caching of objects using Redis.

#Usage
	1. Add a ConnectionStrings to the "app.config" file in the root folder.

	2. Execute "sqlbacked.exe <output_path> <namespace_prefix"

##Example

If you connection strings look like:

	<connectionStrings>
		<add name="MyDb" connectionString="data source=XYZ.database.windows.net,1433;initial catalog=db_name;persist security info=True;user id=username;password=mypassword;multipleactiveresultsets=True" />
	</connectionStrings>

And the namespace of your application is:

	MySolution.MyProject


And you execute:

	sqlbacked.exe . MySolution.MyProject


SqlBacked.exe will create a directory "MyDb" in the current working directory, with a .cs file for each table in the database, with each object living in the namespace:

	MySolution.MyProject.MyDb


Note that "MyDb" is taken from the name of the connection string.

If your database has schemas in it other than "dbo", a table named "mytable" in schema "Core", will be placed in the directory:

	.MyDb\Core\mytable.cs


Which will have the namespace:

	MySolution.MyProject.MyDb.Core



#Operation

SqlBacked goes through any databases listed under the "ConnectionStrings" configuration section of app.config file located at: 
	
	/MSGooroo.SqlBacked/app.config

For each ConnectionString, the app will execute a query to find the available schemas and generate a new class file for each table it finds.  Each Schema will be placed in its own namespace.

