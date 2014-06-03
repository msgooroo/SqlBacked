using System;
using System.IO;

using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoGenerator {
	public enum CacheType {
		None,
		AzureCache,
		Redis
	}

	public class SqlBackedTable {
		public string SchemaName;
		public string TableName;
		public string NamespacePrefix;

		public CacheType Cache;

		public PocoColumn PrimaryKey;
		public List<PocoColumn> Columns;


		public SqlBackedTable(string connectionString, string namespacePrefix, string schemaName, string tableName, CacheType cacheType) {
			SchemaName = schemaName;
			TableName = tableName;
			NamespacePrefix = namespacePrefix;
			Cache = cacheType;

			Columns = new List<PocoColumn>();

			using (var cn = new SqlConnection(connectionString)) {
				cn.Open();

				List<string> schemas = new List<string>();
				using (var cmd = new SqlCommand(File.ReadAllText(@"..\..\sql\columns.sql"), cn)) {
					cmd.Parameters.AddWithValue("@TABLE_SCHEMA", schemaName);
					cmd.Parameters.AddWithValue("@TABLE_NAME", tableName);
					using (SqlDataReader reader = cmd.ExecuteReader()) {
						while (reader.Read()) {
							var c = new PocoColumn(reader);
							Columns.Add(c);

							if (c.IsPrimaryKey) {
								PrimaryKey = c;
							}
						}
					}
				}
			}
		}

		public string GetUpdateSql() {
			if (PrimaryKey == null) {
				return "\t\tpublic string UpdateSql  { get { throw new InvalidOperationException(\"This object has no Primary Key set in the Database\");} }";
			}
			var template = File.ReadAllText("../../templates/UpdateSql.cs");


			string updateSql = string.Join(", ", Columns
				.Where(x => !x.IsPrimaryKey && !x.IsIdentity)
				.Select(x => string.Format("{0} = @{0}", x.ColumnName)).ToArray());


			string code = template
				.Replace("[{TYPE_NAME}]", TableName)
				.Replace("[{SCHEMA_NAME}]", SchemaName)
				.Replace("[{UPDATE_SQL}]", updateSql);

			return code;

		}
		public string GetInsertSql() {
			var template = File.ReadAllText("../../templates/InsertSql.cs");


			string columnList = string.Join(", ", Columns
				.Where(x => !x.IsIdentity)
				.Select(x => x.ColumnName).ToArray());

			string valueList = string.Join(", ", Columns
				.Where(x => !x.IsIdentity)
				.Select(x => "@" + x.ColumnName).ToArray());

			string paramBindings = string.Join("\r\n", Columns
				.Where(x => !x.IsPrimaryKey)
				.Select(x => x.BindParameterToCommand).ToArray()
			);

			string code = template
				.Replace("[{TYPE_NAME}]", TableName)
				.Replace("[{SCHEMA_NAME}]", SchemaName)
				.Replace("[{COLUMN_LIST}]", columnList)
				.Replace("[{VALUE_LIST}]", valueList);
			return code;

		}
		public string GetBindCommand() {
			var template = File.ReadAllText("../../templates/BindCommand.cs");


			string columnList = string.Join(", ", Columns
				.Where(x=>!x.IsIdentity)
				.Select(x => x.ColumnName).ToArray());

			string valueList = string.Join(", ", Columns
				.Where(x => !x.IsIdentity)
				.Select(x => "@" + x.ColumnName).ToArray());

			string paramBindings = string.Join("\r\n", Columns
				.Where(x => !x.IsPrimaryKey)
				.Select(x => x.BindParameterToCommand).ToArray()
			);

			string code = template
				.Replace("[{PARAMETER_BINDINGS}]", paramBindings);

			return code;

		}

		public string GetGetMethods() {
			var template = File.ReadAllText("../../templates/GetDbMethods.cs");

			string code = template
				.Replace("[{TYPE_NAME}]", TableName);

			return code;
		}
		public string GetGetCachedMethods() {
			var template = File.ReadAllText("../../templates/GetCachedMethods.cs");

			string code = template
				.Replace("[{TYPE_NAME}]", TableName);

			return code;
		}

		public string GetBindReader() {
			var template = File.ReadAllText("../../templates/BindReader.cs");

			var bindings = Columns
				.Select(x => x.BindReaderValueToObjectCode)
				.ToArray();

			string code = template
				.Replace("[{TYPE_NAME}]", TableName)
				.Replace("[{SCHEMA_NAME}]", SchemaName)
				.Replace("[{READER_BINDINGS}]", string.Join("\r\n", bindings));

			if (PrimaryKey != null) {
				code = code.Replace("[{PRIMARY_KEY_BINDING}]", string.Format("			_primaryKey = (int) r[\"{0}\"];", PrimaryKey.ColumnName));
			} else {
				code = code.Replace("[{PRIMARY_KEY_BINDING}]", "");
			}

			return code;
		}

		public string GetClass() {
			var template = File.ReadAllText("../../templates/Class.cs");
			var namespaceName = NamespacePrefix;
			if (SchemaName != "dbo") {
				namespaceName += "." + SchemaName;
			}

			var cachingMethods = "";

			if (Cache != CacheType.None && PrimaryKey == null) {
				Console.WriteLine(string.Format("Warning: Unable to use caching for table [{0}] as it does not have an INTEGER identity column", TableName));
			}

			if (Cache != CacheType.None && PrimaryKey != null) {

				cachingMethods = GetGetCachedMethods();
			}

			var properties = string.Join("\r\n", Columns.Select(x => x.PropertyCode));
			string primaryKeyCol = null; 
			if (PrimaryKey != null) {
				properties += "\t\tprivate int _primaryKey;\r\n";
				properties += "\t\tpublic int PrimaryKey { get {return _primaryKey;} }\r\n";

				primaryKeyCol = string.Format("\t\tpublic string PrimaryKeyColumn {{ get {{ return \"{0}\"; }} }}", PrimaryKey.ColumnName);
			} else {
				properties += "\t\tpublic int PrimaryKey { get { throw new InvalidOperationException(\"This object has no Primary Key set in the Database\");} }\r\n";
				primaryKeyCol = "\t\tpublic string PrimaryKeyColumn { get { throw new InvalidOperationException(\"This object has no Primary Key set in the Database\");} }\r\n";
			}

			string code = template
				.Replace("[{TYPE_NAME}]", TableName)
				.Replace("[{NAMESPACE}]", namespaceName)
				.Replace("[{PROPERTIES}]", properties)

				.Replace("[{TABLE_NAME}]", string.Format("\t\tpublic string TableName {{ get {{ return \"{0}\"; }} }}", TableName))
				.Replace("[{SCHEMA_NAME}]", string.Format("\t\tpublic string SchemaName {{ get {{ return \"{0}\"; }} }}", SchemaName))
				.Replace("[{PRIMARY_KEY}]", primaryKeyCol)

				.Replace("[{INSERT_SQL}]", GetInsertSql())
				.Replace("[{UPDATE_SQL}]", GetUpdateSql())

				.Replace("[{BIND_READER}]", GetBindReader())
				.Replace("[{BIND_COMMAND}]", GetBindCommand())

				.Replace("[{GET_METHODS}]", GetGetMethods())
				.Replace("[{GET_CACHE_METHODS}]", cachingMethods);

			if (PrimaryKey == null) {
			}
			return code;

		}


	}
}
