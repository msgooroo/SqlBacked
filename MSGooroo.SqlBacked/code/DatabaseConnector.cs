using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGooroo.SqlBacked {
	public static class DatabaseConnector {


		public static IEnumerable<T> Get<T>(DbConnection cn, string condition, object param) where T : ITableBacked, new() {
			T first = new T();

			string sql = string.Format("SELECT * FROM {0}.{1} WHERE {2} @x", first.SchemaName, first.TableName, condition);
			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = sql;
				cmd.Parameters.Add(GetParameter(cmd, "@x", param));
				using (var r = cmd.ExecuteReader()) {
					while (r.Read()) {
						T me = new T();
						me.BindReader(r);
						yield return me;
					}
				}
			}
		}

		public static IEnumerable<T> GetSql<T>(DbConnection cn, string sql, object ps) where T : ITableBacked, new() {
			T first = new T();

			using (var cmd = cn.CreateCommand()) {
				cmd.CommandText = sql;
				if (ps != null) {
					foreach (var p in ps.GetType().GetProperties()) {
						cmd.Parameters.Add(GetParameter(cmd, "@" + p.Name, p.GetValue(ps)));
					}
				}
				using (var r = cmd.ExecuteReader()) {
					while (r.Read()) {
						T me = new T();
						me.BindReader(r);
						yield return me;
					}
				}
			}
		}
		public static T Get<T>(DbConnection cn, int primaryKey) where T : ITableBacked, new() {
			T first = new T();
			if (first.PrimaryKeyColumn == null) {
				throw new InvalidOperationException("Using Get with a Primary Key requires that the ITableBacked has a Primary Key defined");
			
			}
			return Get<T>(cn, first.PrimaryKeyColumn + "=", primaryKey).FirstOrDefault();
			
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

		public static int Insert<T>(this T obj, DbConnection cn) where T : ITableBacked, new() {
			return Insert<T>(obj, cn);
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

		public static int Update<T>(this T obj, DbConnection cn) where T : ITableBacked, new() {
			return Update<T>(obj, cn, null);
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

		public static int Delete<T>(this T obj, DbConnection cn) where T : ITableBacked, new() {
			return Delete<T>(obj, cn, null);
		}


		public static DbParameter GetParameter(DbCommand cmd, string name, object val) {
			DbParameter p = cmd.CreateParameter();
			p.ParameterName = name;
			p.Value = val == null ? (object)DBNull.Value : val;
			return p;
		}



	}
}
