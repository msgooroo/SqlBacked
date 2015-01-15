using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GoorooIO.SqlBacked {
	public static class DatabaseConnector {


		public static DbCommand GetCommand<T>(this DbConnection cn, string condition, object param) where T : ITableBacked, new() {
			var obj = new T();

			string sql = string.Format("SELECT * FROM {0}.{1} WHERE {2} @x", obj.SchemaName, obj.TableName, condition);
			var cmd = cn.CreateCommand();

			cmd.CommandText = sql;
			cmd.Parameters.Add(GetParameter(cmd, "@x", param));

			return cmd;
		}

		public static DbCommand GetCommand<T>(this DbConnection cn, int primaryKey) where T : ITableBacked, new() {
			var obj = new T();

			return GetCommand<T>(cn, obj.PrimaryKeyColumn + "=", primaryKey);
		}


		public static DbCommand GetSqlCommand<T>(this DbConnection cn, string sql, object ps) where T : ITableBacked, new() {
			var cmd = cn.CreateCommand();
			cmd.CommandText = sql;
			if (ps != null) {
				foreach (var p in ps.GetType().GetProperties()) {
					cmd.Parameters.Add(GetParameter(cmd, "@" + p.Name, p.GetValue(ps)));
				}
			}
			return cmd;
		}

		public static List<T> GetList<T>(DbCommand cmd) where T : ITableBacked, new() {
			var values = new List<T>();
			using (var r = cmd.ExecuteReader()) {
				while (r.Read()) {
					T me = new T();
					me.BindReader(r);
					values.Add(me);
				}
			}

			return values;
		}

		public static T GetSingle<T>(DbCommand cmd) where T : ITableBacked, new() {
			var value = new T();
			using (var r = cmd.ExecuteReader()) {
				if (r.Read()) {
					value.BindReader(r);
				}
			}
			return value;
		}





		/// <summary>
		/// Executes an SQL Command using the supplied connection and sql query.
		/// The object, "ps" will be reflected such that its properties are bound
		/// as named parameters to the query.
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static int ExecuteSql(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = sql;
				cmd.CommandTimeout = 60000;
				if (ps != null) {
					foreach (var p in ps.GetType().GetProperties()) {
						cmd.Parameters.Add(GetParameter(cmd, "@" + p.Name, p.GetValue(ps)));
					}
				}
				return cmd.ExecuteNonQuery();
			}
		}

		public static async Task<int> ExecuteSqlAsync(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = sql;
				cmd.CommandTimeout = 60000;
				if (ps != null) {
					foreach (var p in ps.GetType().GetProperties()) {
						cmd.Parameters.Add(GetParameter(cmd, "@" + p.Name, p.GetValue(ps)));
					}
				}
				return await cmd.ExecuteNonQueryAsync();
			}
		}


		public static List<T> Get<T>(DbConnection cn, string condition, object param) where T : ITableBacked, new() {
			var values = new List<T>();
			using (var cmd = GetCommand<T>(cn, condition, param)) {
				return GetList<T>(cmd);
			}
		}

		public static T Get<T>(DbConnection cn, int primaryKey) where T : ITableBacked, new() {
			T first = new T();

			var values = new List<T>();
			using (var cmd = GetCommand<T>(cn, first.PrimaryKeyColumn + "=", primaryKey)) {
				return GetSingle<T>(cmd);
			}

		}

		public static List<T> GetSql<T>(DbConnection cn, string sql, object ps) where T : ITableBacked, new() {
			var values = new List<T>();
			using (var cmd = GetSqlCommand<T>(cn, sql, ps)) {
				return GetList<T>(cmd);
			}
		}


		public static int Insert<T>(this T obj, DbConnection cn, DbTransaction txn) where T : ITableBacked, new() {
			int primaryKey = -1;
			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = obj.InsertSql;
				cmd.Transaction = txn;
				obj.BindCommand(cmd);
				using (var r = cmd.ExecuteReader()) {
					if (r.Read() && r[0] != DBNull.Value) {
						primaryKey = (int)(decimal)r[0];
					}
				}
			}
			return primaryKey;
		}
		public static async Task<int> InsertAsync<T>(this T obj, DbConnection cn, DbTransaction txn) where T : ITableBacked, new() {
			int primaryKey = -1;
			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = obj.InsertSql;
				cmd.Transaction = txn;
				obj.BindCommand(cmd);
				using (var r = await cmd.ExecuteReaderAsync()) {
					if (await r.ReadAsync() && r[0] != DBNull.Value) {
						primaryKey = (int)(decimal)r[0];
					}
				}
			}
			return primaryKey;
		}

		public static int Insert<T>(this T obj, DbConnection cn) where T : ITableBacked, new() {
			return Insert<T>(obj, cn, null);
		}

		public static async Task<int> InsertAsync<T>(this T obj, DbConnection cn) where T : ITableBacked, new() {
			return await InsertAsync<T>(obj, cn, null);
		}

		public static int Update<T>(this T obj, DbConnection cn, DbTransaction txn) where T : ITableBacked, new() {
			if (obj.PrimaryKeyColumn == null) {
				throw new InvalidOperationException("Using Update requires that the ITableBacked has a Primary Key defined");
			}

			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = obj.UpdateSql;
				cmd.Transaction = txn;
				obj.BindCommand(cmd);

				// Make sure we bind the WHERE condition 
				cmd.Parameters.Add(GetParameter(cmd, "PkColumn", obj.PrimaryKey));

				cmd.ExecuteNonQuery();
			}
			return obj.PrimaryKey;
		}

		public static async Task<int> UpdateAsync<T>(this T obj, DbConnection cn, DbTransaction txn) where T : ITableBacked, new() {
			if (obj.PrimaryKeyColumn == null) {
				throw new InvalidOperationException("Using Update requires that the ITableBacked has a Primary Key defined");
			}

			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = obj.UpdateSql;
				cmd.Transaction = txn;
				obj.BindCommand(cmd);

				// Make sure we bind the WHERE condition 
				cmd.Parameters.Add(GetParameter(cmd, "PkColumn", obj.PrimaryKey));

				await cmd.ExecuteNonQueryAsync();
			}
			return obj.PrimaryKey;
		}

		public static int Update<T>(this T obj, DbConnection cn) where T : ITableBacked, new() {
			return Update<T>(obj, cn, null);
		}
		public static async Task<int> UpdateAsync<T>(this T obj, DbConnection cn) where T : ITableBacked, new() {
			return await UpdateAsync<T>(obj, cn, null);
		}

		public static int Delete<T>(T obj, DbConnection cn, DbTransaction txn) where T : ITableBacked, new() {
			if (obj.PrimaryKeyColumn == null) {
				throw new InvalidOperationException("Using Delete requires that the ITableBacked has a Primary Key defined");
			}

			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = string.Format("DELETE FROM {0}.{1} WHERE {2}=@PkColumn", obj.SchemaName, obj.TableName, obj.PrimaryKeyColumn);
				cmd.Transaction = txn;

				// Make sure we bind the WHERE condition 
				cmd.Parameters.Add(GetParameter(cmd, "PkColumn", obj.PrimaryKey));
				cmd.ExecuteNonQuery();
			}

			return obj.PrimaryKey;
		}

		public static async Task<int> DeleteAsync<T>(T obj, DbConnection cn, DbTransaction txn) where T : ITableBacked, new() {
			if (obj.PrimaryKeyColumn == null) {
				throw new InvalidOperationException("Using Delete requires that the ITableBacked has a Primary Key defined");
			}

			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = string.Format("DELETE FROM {0}.{1} WHERE {2}=@PkColumn", obj.SchemaName, obj.TableName, obj.PrimaryKeyColumn);
				cmd.Transaction = txn;

				// Make sure we bind the WHERE condition 
				cmd.Parameters.Add(GetParameter(cmd, "PkColumn", obj.PrimaryKey));
				await cmd.ExecuteNonQueryAsync();
			}

			return obj.PrimaryKey;
		}


		public static int Delete<T>(this T obj, DbConnection cn) where T : ITableBacked, new() {
			return Delete<T>(obj, cn, null);
		}

		public static async Task<int> DeleteAsync<T>(this T obj, DbConnection cn) where T : ITableBacked, new() {
			return await DeleteAsync<T>(obj, cn, null);
		}


		public static DbParameter GetParameter(DbCommand cmd, string name, object val) {
			DbParameter p = cmd.CreateParameter();
			p.ParameterName = name;
			p.Value = val == null ? (object)DBNull.Value : val;
			return p;
		}


		public static string DumpJsonRows(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = sql;
				cmd.CommandTimeout = 10;
				if (ps != null) {
					foreach (var p in ps.GetType().GetProperties()) {
						cmd.Parameters.Add(GetParameter(cmd, "@" + p.Name, p.GetValue(ps)));
					}
				}
				using (var reader = cmd.ExecuteReader()) {
					// Field names
					List<string> columnNames =
						Enumerable.Range(0, reader.FieldCount)
							.Select(x => reader.GetName(x))
							.ToList();
					List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
					while (reader.Read()) {
						Dictionary<string, string> rowData = new Dictionary<string, string>();
						for (var i = 0; i < reader.FieldCount; i++) {
							if (reader[i].GetType() == typeof(DateTime)) {
								// Use ISO time
								rowData[columnNames[i]] = ((DateTime)reader[i]).ToString("s");
							} else {
								rowData[columnNames[i]] = reader[i].ToString();
							}
						}
						data.Add(rowData);
					}
					return JsonConvert.SerializeObject(new { ColumnNames = columnNames, Rows = data });
				}
			}
		}

		public static DataTable DumpDataTable(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = sql;
				if (ps != null) {
					foreach (var p in ps.GetType().GetProperties()) {
						cmd.Parameters.Add(GetParameter(cmd, "@" + p.Name, p.GetValue(ps)));
					}
				}
				using (var reader = cmd.ExecuteReader()) {
					// Field names
					var table = new DataTable();
					table.Load(reader);
					return table;
				}
			}
		}

		#region async

		public static async Task<T> GetAsync<T>(DbConnection cn, int primaryKey) where T : ITableBacked, new() {
			T first = new T();
			if (first.PrimaryKeyColumn == null) {
				throw new InvalidOperationException("Using Get with a Primary Key requires that the ITableBacked has a Primary Key defined");

			}
			var results = await GetAsync<T>(cn, first.PrimaryKeyColumn + "=", primaryKey);

			return results.FirstOrDefault();
		}



		public static async Task<List<T>> GetAsync<T>(DbConnection cn, string condition, object param) where T : ITableBacked, new() {
			T first = new T();
			var values = new List<T>();
			string sql = string.Format("SELECT * FROM {0}.{1} WHERE {2} @x", first.SchemaName, first.TableName, condition);
			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = sql;
				cmd.Parameters.Add(GetParameter(cmd, "@x", param));
				using (var r = await cmd.ExecuteReaderAsync()) {
					while (await r.ReadAsync()) {
						T me = new T();
						me.BindReader(r);
						values.Add(me);
					}
				}
			}
			return values;
		}

		public static async Task<List<T>> GetSqlAsync<T>(DbConnection cn, string sql, object ps) where T : ITableBacked, new() {
			T first = new T();

			var values = new List<T>();
			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = sql;
				if (ps != null) {
					foreach (var p in ps.GetType().GetProperties()) {
						cmd.Parameters.Add(GetParameter(cmd, "@" + p.Name, p.GetValue(ps)));
					}
				}
				using (var r = await cmd.ExecuteReaderAsync()) {
					while (await r.ReadAsync()) {
						T me = new T();
						me.BindReader(r);
						values.Add(me);
					}
				}
			}
			return values;

		}


		public static async Task<DataTable> DumpDataTableAsync(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = sql;
				if (ps != null) {
					foreach (var p in ps.GetType().GetProperties()) {
						cmd.Parameters.Add(GetParameter(cmd, "@" + p.Name, p.GetValue(ps)));
					}
				}
				using (var reader = await cmd.ExecuteReaderAsync()) {
					// Field names
					var table = new DataTable();
					table.Load(reader);
					return table;
				}
			}
		}

		public static async Task<string> DumpJsonRowsAsync(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = sql;
				cmd.CommandTimeout = 10;
				if (ps != null) {
					foreach (var p in ps.GetType().GetProperties()) {
						cmd.Parameters.Add(GetParameter(cmd, "@" + p.Name, p.GetValue(ps)));
					}
				}
				using (var reader = await cmd.ExecuteReaderAsync()) {
					// Field names
					List<string> columnNames =
						Enumerable.Range(0, reader.FieldCount)
							.Select(x => reader.GetName(x))
							.ToList();
					List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
					while (await reader.ReadAsync()) {
						Dictionary<string, string> rowData = new Dictionary<string, string>();
						for (var i = 0; i < reader.FieldCount; i++) {
							if (reader[i].GetType() == typeof(DateTime)) {
								// Use ISO time
								rowData[columnNames[i]] = ((DateTime)reader[i]).ToString("s");
							} else {
								rowData[columnNames[i]] = reader[i].ToString();
							}
						}
						data.Add(rowData);
					}
					return JsonConvert.SerializeObject(new { ColumnNames = columnNames, Rows = data });
				}
			}
		}
		#endregion async

	}
}
