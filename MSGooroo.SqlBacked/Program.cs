using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading.Tasks;
using System.Data.Common;
using System.IO;

namespace PocoGenerator {
	public class Program {


		static void Main(string[] args) {

			if (args.Length != 2) {
				Console.WriteLine("Usage: sqlbacked.exe <output_path> <namespace>");
				Console.WriteLine("\t\toutput_path: The path where you want the SqlBacked class files to be saved");
				Console.WriteLine("\t\tnamespace:	The prefix of the namespace for your new SqlBacked classes");
				return;
			}

			string path = args[0];
			string namespacePrefix = args[1];

			bool found = false;
			foreach (ConnectionStringSettings s in ConfigurationManager.ConnectionStrings) {
				if (s.Name != "LocalSqlServer") {
					Console.WriteLine("Writing: {0}", s.Name);
					BuildClasses(s.ConnectionString, path + "\\" + s.Name, namespacePrefix + "." + s.Name);
					found = true;
				}
			}

			if (!found) {
				Console.WriteLine("No connection strings were found in the App.config, no SqlBacked objects written");
			}
		}


		private static void BuildClasses(string connectionString, string path, string namespacePrefix) {

			using (var cn = new SqlConnection(connectionString)) {
				cn.Open();

				List<string> schemas = new List<string>();
				using (var cmd = new SqlCommand(File.ReadAllText(@"..\..\sql\schemas.sql"), cn)) {
					using (SqlDataReader reader = cmd.ExecuteReader()) {
						while (reader.Read()) {
							schemas.Add(reader["SCHEMA_NAME"] as string);
						}
					}
				}
				foreach (var schema in schemas) {

					List<string> tables = new List<string>();
					using (var cmd = new SqlCommand(File.ReadAllText(@"..\..\sql\tables.sql"), cn)) {
						cmd.Parameters.AddWithValue("@TABLE_SCHEMA", schema);
						using (SqlDataReader reader = cmd.ExecuteReader()) {
							while (reader.Read()) {
								string tableName = reader["TABLE_NAME"] as string;

								SqlBackedTable t = new SqlBackedTable(connectionString, namespacePrefix, schema, tableName,  CacheType.Redis);
								string directoryPath = string.Format("{0}\\{1}\\", path, schema);
								string classPath = string.Format("{0}{1}.cs", directoryPath, tableName);

								Directory.CreateDirectory(directoryPath);

								Console.WriteLine("Wrote: " + classPath);

								File.WriteAllText(classPath, t.GetClass());
							}
						}
					}
				}

			}
			try {
				// Copy over the other files we need
				File.Copy(@"..\..\Code\ITableBacked.cs", path + @"\ITableBacked.cs", true);
				File.Copy(@"..\..\Code\ICacheProvider.cs", path + @"\ICacheProvider.cs", true);
				File.Copy(@"..\..\Code\CacheConnector.cs", path + @"\CacheConnector.cs", true);
				File.Copy(@"..\..\Code\DatabaseConnector.cs", path + @"\DatabaseConnector.cs", true);
				File.Copy(@"..\..\Code\CacheProviders\RedisCacheProvider.cs", path + @"\RedisCacheProvider.cs", true);
			} catch {
			}

		}
	}
}
