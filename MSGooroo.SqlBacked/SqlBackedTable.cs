using System;
using System.IO;

using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoGenerator {
	public class SqlBackedTable {
		public string SchemaName;
		public string TableName;
		public string NamespacePrefix;

		public bool IncludeCache;

		public PocoColumn PrimaryKey;
		public List<PocoColumn> Columns;

		public string DatabaseName;

		public SqlBackedTable(string connectionString, string namespacePrefix, string schemaName, string tableName, bool includeCache) {


			SchemaName = schemaName;
			TableName = tableName;
			NamespacePrefix = namespacePrefix;
			IncludeCache = includeCache;

			Columns = new List<PocoColumn>();

			using (var cn = new SqlConnection(connectionString)) {
				DatabaseName = cn.Database;
				cn.Open();

				List<string> schemas = new List<string>();
				string path = Program.MapPath(@"..\sql\columns.sql");
				using (var cmd = new SqlCommand(File.ReadAllText(path), cn)) {
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
			string path = Program.MapPath(@"..\templates\UpdateSql.cs");
			var template = File.ReadAllText(path);


			string updateSql = string.Join(", ", Columns
				.Where(x => !x.IsPrimaryKey && !x.IsIdentity)
				.Select(x => string.Format("{0} = @{0}", x.ColumnName)).ToArray());


			string code = template
				.Replace("[{TYPE_NAME}]", TableName)
				.Replace("[{SCHEMA_NAME}]", SchemaName)
				.Replace("[{PKCOLUMN}]", PrimaryKey.ColumnName)
				.Replace("[{UPDATE_SQL}]", updateSql);

			return code;

		}
		public string GetInsertSql() {
			string path = Program.MapPath(@"..\templates\InsertSql.cs");
			var template = File.ReadAllText(path);


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
			string path = Program.MapPath(@"..\templates\BindCommand.cs");
			var template = File.ReadAllText(path);


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
			string path = Program.MapPath(@"..\templates\GetDbMethods.cs");
			var template = File.ReadAllText(path);

			string code = template
				.Replace("[{TYPE_NAME}]", TableName);

			return code;
		}
		public string GetGetCachedMethods() {
			string path = Program.MapPath(@"..\templates\GetCachedMethods.cs");
			var template = File.ReadAllText(path);

			string code = template
				.Replace("[{TYPE_NAME}]", TableName);

			return code;
		}
		public string GetGetCachedSqlMethods() {
			string path = Program.MapPath(@"..\templates\GetCachedSqlMethods.cs");
			var template = File.ReadAllText(path);

			string code = template
				.Replace("[{TYPE_NAME}]", TableName);

			return code;
		}

		public string GetBindReader() {
			string path = Program.MapPath(@"..\templates\BindReader.cs");
			var template = File.ReadAllText(path);

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
			string path = Program.MapPath(@"..\templates\Class.cs");
			var template = File.ReadAllText(path);

			var namespaceName = NamespacePrefix;
			if (SchemaName != "dbo") {
				namespaceName += "." + SchemaName;
			}

			var cachingMethods = "";

			if (this.IncludeCache && PrimaryKey == null) {
				Console.WriteLine(string.Format("Warning: Unable to use caching for table [{0}] as it does not have an INTEGER identity column", TableName));
			}

			if (this.IncludeCache && PrimaryKey != null) {

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
				primaryKeyCol = "\t\tpublic string PrimaryKeyColumn { get { return null;} }\r\n";
			}

			string code = template
				.Replace("[{TIME_STAMP}]", DateTime.Now.ToString())
				.Replace("[{DATABASE_NAME}]", DatabaseName)
				.Replace("[{TABLE_NAME}]", TableName)
				.Replace("[{SCHEMA_NAME}]", SchemaName)
				.Replace("[{NAMESPACE}]", namespaceName)
				.Replace("[{PROPERTIES}]", properties)

				.Replace("[{TABLE_NAME_PROPERTY}]", string.Format("\t\tpublic string TableName {{ get {{ return \"{0}\"; }} }}", TableName))
				.Replace("[{SCHEMA_NAME_PROPERTY}]", string.Format("\t\tpublic string SchemaName {{ get {{ return \"{0}\"; }} }}", SchemaName))
				.Replace("[{PRIMARY_KEY_PROPERTY}]", primaryKeyCol)

				.Replace("[{INSERT_SQL}]", GetInsertSql())
				.Replace("[{UPDATE_SQL}]", GetUpdateSql())

				.Replace("[{BIND_READER}]", GetBindReader())
				.Replace("[{BIND_COMMAND}]", GetBindCommand())

				.Replace("[{GET_METHODS}]", GetGetMethods())
				.Replace("[{GET_CACHE_METHODS}]", cachingMethods)
				.Replace("[{GET_CACHE_SQL_METHODS}]", GetGetCachedSqlMethods());

			return code;

		}


	}
}
