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

		public enum CacheType {
			None,
			AzureCache,
			InProcess,
			Redis
		}


		public static string MapPath(string newpath) {
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, newpath);
		}
		static void PrintUsage() {
			Console.WriteLine("Usage: sqlbacked.exe <output_path> <namespace> <cache>");
			Console.WriteLine("\t\toutput_path: The path where you want the SqlBacked class files to be saved");
			Console.WriteLine("\t\tnamespace:	The prefix of the namespace for your new SqlBacked classes");
			Console.WriteLine("\t\tcache:	Either 'none' for no caching methods or 'redis' for caching methods, including the redis adapter");
		}

		static void Main(string[] args) {

			if (args.Length != 3) {
				PrintUsage();
				return;
			}

			string path = args[0];
			string namespacePrefix = args[1];
			CacheType type = CacheType.None;
			if (args[2] == "redis") {
				type = CacheType.Redis;
			} else if (args[2] == "azure") {
				type = CacheType.AzureCache;

			} else if (args[2] == "inproc") {
				type = CacheType.InProcess;

			} else if (args[2] != "none") {
				Console.WriteLine("Unknown / Invalid Cache Type, please use one of ['none', 'redis', 'azure'].\n");
				PrintUsage();
				return;
			}

			bool found = false;
			foreach (ConnectionStringSettings s in ConfigurationManager.ConnectionStrings) {
				if (s.Name != "LocalSqlServer") {
					Console.WriteLine("Writing: {0}", s.Name);
					BuildClasses(s.ConnectionString, path + "\\" + s.Name, namespacePrefix + "." + s.Name, type);
					found = true;
				}
			}

			if (!found) {
				Console.WriteLine("No connection strings were found in the App.config, no SqlBacked objects written");
			}
		}


		private static void BuildClasses(string connectionString, string path, string namespacePrefix, CacheType cacheType) {
			try {
				using (var cn = new SqlConnection(connectionString)) {
					cn.Open();

					List<string> schemas = new List<string>();
					using (var cmd = new SqlCommand(File.ReadAllText(MapPath(@"..\sql\schemas.sql")), cn)) {
						using (SqlDataReader reader = cmd.ExecuteReader()) {
							while (reader.Read()) {
								schemas.Add(reader["SCHEMA_NAME"] as string);
							}
						}
					}
					foreach (var schema in schemas) {

						List<string> tables = new List<string>();
						using (var cmd = new SqlCommand(File.ReadAllText(MapPath(@"..\sql\tables.sql")), cn)) {
							cmd.Parameters.AddWithValue("@TABLE_SCHEMA", schema);
							using (SqlDataReader reader = cmd.ExecuteReader()) {
								while (reader.Read()) {
									string tableName = reader["TABLE_NAME"] as string;

									SqlBackedTable t = new SqlBackedTable(connectionString, namespacePrefix, schema, tableName, cacheType != CacheType.None);
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
				File.Copy(MapPath(@"..\Code\ITableBacked.cs"), path + @"\ITableBacked.cs", true);
				File.Copy(MapPath(@"..\Code\PerformanceLogger.cs"), path + @"\PerformanceLogger.cs", true);
				File.Copy(MapPath(@"..\Code\ICacheProvider.cs"), path + @"\ICacheProvider.cs", true);
				File.Copy(MapPath(@"..\Code\CacheConnector.cs"), path + @"\CacheConnector.cs", true);
				File.Copy(MapPath(@"..\Code\DatabaseConnector.cs"), path + @"\DatabaseConnector.cs", true);
				File.Copy(MapPath(@"..\Code\BatchContext.cs"), path + @"\BatchContext.cs", true);

				if (cacheType == CacheType.Redis) {
					File.Copy(MapPath(@"..\Code\CacheProviders\RedisCacheProvider.cs"), path + @"\RedisCacheProvider.cs", true);
				}

				if (cacheType == CacheType.AzureCache) {
					File.Copy(MapPath(@"..\Code\CacheProviders\AzureCacheProvider.cs"), path + @"\AzureCacheProvider.cs", true);
				}
				if (cacheType == CacheType.Redis) {
					File.Copy(MapPath(@"..\Code\CacheProviders\MemoryCacheProvider.cs"), path + @"\MemoryCacheProvider.cs", true);
				}

			} catch (Exception ex) {
				Console.WriteLine("An exception occurred");
				Console.WriteLine(ex.Message);
				Console.WriteLine("---------");
				Console.WriteLine(ex.StackTrace);
			}

			
		}
	}
}
