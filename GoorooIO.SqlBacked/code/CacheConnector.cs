using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoorooIO.SqlBacked {

	public static class CacheConnector {





		/// <summary>
		/// Gets a list of items using the SQL supplied, trying the cache first, then the database.
		/// The object passed in as "ps" will be reflected on, and its properties used as parameters.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cn"></param>
		/// <param name="cache"></param>
		/// <param name="condition"></param>
		/// <param name="param">An object whose properties will be used as named parameters</param>
		/// <returns></returns>

		public static List<T> GetSqlCached<T>(DbConnection cn, ICacheProvider cache, string sql, object ps) where T : class, ITableBacked, new() {

			T first = new T();

			string cacheKey = first.SqlCacheKey(sql, ps);
			var items = GetCachedOnly<T>(cn, cache, cacheKey);
			if (items != null) {
				return items;
			} else {
				return GetSqlAndRefreshCached<T>(cn, cache, sql, ps);
			}
		}
		//public static async Task<List<T>> GetSqlCachedAsync<T>(DbConnection cn, ICacheProvider cache, string sql, object ps) where T : class, ITableBacked, new() {

		//	T first = new T();

		//	string cacheKey = first.SqlCacheKey(sql, ps);
		//	var items = await GetCachedOnlyAsync<T>(cn, cache, cacheKey);
		//	if (items != null) {
		//		return items;
		//	} else {
		//		return await GetSqlAndRefreshCachedAsync<T>(cn, cache, sql, ps);
		//	}
		//}

		/// <summary>
		/// Gets a list of items, trying the cache first, then the database
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cn"></param>
		/// <param name="cache"></param>
		/// <param name="condition"></param>
		/// <param name="param"></param>
		/// <returns></returns>
		public static List<T> GetCached<T>(DbConnection cn, ICacheProvider cache, string condition, object param) where T : class, ITableBacked, new() {
			T first = new T();

			string cacheKey = first.CacheKey(condition, param);
			var items = GetCachedOnly<T>(cn, cache, cacheKey);
			if (items != null) {
				return items;
			} else {
				return GetAndRefreshCached<T>(cn, cache, condition, param);
			}
		}
		//public static async Task<List<T>> GetCachedAsync<T>(DbConnection cn, ICacheProvider cache, string condition, object param) where T : class, ITableBacked, new() {
		//	T first = new T();

		//	string cacheKey = first.CacheKey(condition, param);
		//	var items = await GetCachedOnlyAsync<T>(cn, cache, cacheKey);
		//	if (items != null) {
		//		return items;
		//	} else {
		//		return await GetAndRefreshCachedAsync<T>(cn, cache, condition, param);
		//	}
		//}

		/// <summary>
		/// Get an item, trying the cache first, then the database
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cn"></param>
		/// <param name="cache"></param>
		/// <param name="primaryKey"></param>
		/// <returns></returns>
		public static T GetCached<T>(DbConnection cn, ICacheProvider cache, int primaryKey) where T : class, ITableBacked, new() {
			T first = new T();

			var cacheKey = first.SingleCacheKey(primaryKey);

			if (cache != null) {
				var item = cache.Get<T>(cacheKey);
				if (item != null) {
					return item;
				}
			}
			return GetAndRefreshCached<T>(cn, cache, primaryKey);
		}
		//public static async Task<T> GetCachedAsync<T>(DbConnection cn, ICacheProvider cache, int primaryKey) where T : class, ITableBacked, new() {
		//	T first = new T();

		//	var cacheKey = first.SingleCacheKey(primaryKey);

		//	if (cache != null) {
		//		var item = await cache.GetAsync<T>(cacheKey);
		//		if (item != null) {
		//			return item;
		//		}
		//	}
		//	return await GetAndRefreshCachedAsync<T>(cn, cache, primaryKey);
		//}


		/// <summary>
		/// Try to get a list of values by only going to the cache.  Returns null if the list
		/// is not in the cache.  This will still go to the database to fill in any holes in the
		/// list (e.g. where a list is in the cache, but an element has been ejected).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cn">Database connected to use to fill in any holes in the list.</param>
		/// <param name="cache">The cache to query</param>
		/// <param name="cacheKey">The key for the list we are querying</param>
		/// <returns></returns>
		private static List<T> GetCachedOnly<T>(DbConnection cn, ICacheProvider cache, string cacheKey) where T : class, ITableBacked, new() {
			if (cache == null) {
				return null;
			}
			return cache.Get<List<T>>(cacheKey);
		}


		//private static async Task<List<T>> GetCachedOnlyAsync<T>(DbConnection cn, ICacheProvider cache, string cacheKey) where T : class, ITableBacked, new() {
		//	if (cache == null) {
		//		return null;
		//	}
		//	return await cache.GetAsync<List<T>>(cacheKey);
		//}


		/// <summary>
		/// Gets the item from the database, and populates the cache.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cn"></param>
		/// <param name="cache"></param>
		/// <param name="primaryKey"></param>
		/// <returns></returns>
		public static T GetAndRefreshCached<T>(DbConnection cn, ICacheProvider cache, int primaryKey) where T : class, ITableBacked, new() {
			T first = new T();
			var cacheKey = first.SingleCacheKey(primaryKey);

			var db = DatabaseConnector.Get<T>(cn, primaryKey);
			if (cache != null) {
				Task.Run(() => {
					try {
						cache.Set(cacheKey, db);
						cache.SetExpiry(cacheKey, new TimeSpan(0, 1, 0, 0));
					} catch { }
				});
			}
			return db;
		}

		//public static async Task<T> GetAndRefreshCachedAsync<T>(DbConnection cn, ICacheProvider cache, int primaryKey) where T : class, ITableBacked, new() {
		//	T first = new T();
		//	var cacheKey = first.SingleCacheKey(primaryKey);

		//	var db = await DatabaseConnector.GetAsync<T>(cn, primaryKey);
		//	if (cache != null) {

		//		await cache.SetAsync(cacheKey, db);
		//	}
		//	return db;
		//}




		/// <summary>
		/// Gets the list of items from the database and populates the cache.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cn"></param>
		/// <param name="cache"></param>
		/// <param name="condition"></param>
		/// <param name="param"></param>
		/// <returns></returns>
		public static List<T> GetAndRefreshCached<T>(DbConnection cn, ICacheProvider cache, string condition, object param) where T : class, ITableBacked, new() {
			T first = new T();
			string cacheKey = first.CacheKey(condition, param);
			var items = DatabaseConnector.Get<T>(cn, condition, param);

			CacheItems(cache, cacheKey, items);

			return items;
		}
		//public static async Task<List<T>> GetAndRefreshCachedAsync<T>(DbConnection cn, ICacheProvider cache, string condition, object param) where T : class, ITableBacked, new() {
		//	T first = new T();
		//	string cacheKey = first.CacheKey(condition, param);
		//	var items = await DatabaseConnector.GetAsync<T>(cn, condition, param);

		//	CacheItems(cache, cacheKey, items);

		//	return items;
		//}


		/// <summary>
		/// Gets the list of items from the database from an SQL command and populates the cache.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cn"></param>
		/// <param name="cache"></param>
		/// <param name="condition"></param>
		/// <param name="param"></param>
		/// <returns></returns>
		public static List<T> GetSqlAndRefreshCached<T>(DbConnection cn, ICacheProvider cache, string sql, object ps) where T : class, ITableBacked, new() {
			T first = new T();

			string cacheKey = first.SqlCacheKey(sql, ps);
			var items = DatabaseConnector.GetSql<T>(cn, sql, ps).ToList();
			CacheItems(cache, cacheKey, items);

			return items;
		}
		//public static async Task<List<T>> GetSqlAndRefreshCachedAsync<T>(DbConnection cn, ICacheProvider cache, string sql, object ps) where T : class, ITableBacked, new() {
		//	T first = new T();

		//	string cacheKey = first.SqlCacheKey(sql, ps);
		//	var items = await DatabaseConnector.GetSqlAsync<T>(cn, sql, ps);
		//	await CacheItemsAsync(cache, cacheKey, items);

		//	return items;
		//}

		/// <summary>
		///		Caches a list of items from the database, with the list being stored
		///		as a list of the Cache Keys, and the individual values being saved 
		///		with their own cache keys (so they will be available for single value cache requests).
		///		The Put commands are issued in their own tasks in the background.
		/// </summary>
		/// <param name="cache">The cache to save the values too</param>
		/// <param name="cacheKey">The cache key for the list of keys</param>
		/// <param name="items">The items we want to save in the cache</param>
		/// <returns></returns>
		public static void CacheItems<T>(ICacheProvider cache, string cacheKey, List<T> items) where T : class, ITableBacked, new() {
			if (cache != null) {

				Task.Run(() => {
					try {
						cache.Set(cacheKey, items);
						cache.SetExpiry(cacheKey, new TimeSpan(0, 1, 0, 0));
					} catch { }
				});
			}

		}

		//public static async Task CacheItemsAsync<T>(ICacheProvider cache, string cacheKey, List<T> items) where T : class, ITableBacked, new() {
		//	if (cache != null) {

		//		T first = new T();
		//		if (first.PrimaryKeyColumn == null) {
		//			// Cache the whole object in one go, since we dont have
		//			// primary keys to use for cache keys for individual items.
		//			await cache.SetAsync(cacheKey, items);

		//			return;

		//		}
		//		return;
		//	}
		//}

		/// <summary>
		/// Remove the reference to this item from the cache
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="cache"></param>
		public static void Uncache<T>(this T obj, ICacheProvider cache) where T : class, ITableBacked, new() {
			if (cache != null) {
				string s = obj.SingleCacheKey<T>();
				cache.Remove(s);
			}

		}
		//public static async Task UncacheAsync<T>(this T obj, ICacheProvider cache) where T : class, ITableBacked, new() {
		//	if (cache != null) {
		//		string s = obj.SingleCacheKey<T>();
		//		await cache.RemoveAsync(s);
		//	}

		//}
		/// <summary>
		/// Delete the object from the database and also remove it from the cache
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="cn"></param>
		/// <param name="cache"></param>
		public static void DeleteCached<T>(this T obj, DbConnection cn, ICacheProvider cache) where T : class, ITableBacked, new() {
			obj.Delete(cn);
			obj.Uncache(cache);
		}
		//public static async Task DeleteCachedAsync<T>(this T obj, DbConnection cn, ICacheProvider cache) where T : class, ITableBacked, new() {
		//	await obj.DeleteAsync(cn);
		//	await obj.UncacheAsync(cache);
		//}
		/// <summary>
		/// Execute the update command in the database, and update the cache to the current version
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="cn"></param>
		/// <param name="cache"></param>
		public static void UpdateCached<T>(this T obj, DbConnection cn, ICacheProvider cache) where T : class, ITableBacked, new() {
			obj.Update(cn);
			if (cache != null) {
				cache.Set<T>(obj.SingleCacheKey(), obj);
			}
		}



		/// <summary>
		///		Gets the cache key for a single value
		/// </summary>
		/// <param name="primaryKey">The primary key of the database table</param>
		public static string SingleCacheKey<T>(this T obj, int primaryKey) where T : class, ITableBacked, new() {
			return string.Format("{0}|one|{1}", obj.TableName, primaryKey);
		}

		/// <summary>
		///		Gets the cache key for a single value
		/// </summary>
		/// <param name="primaryKey">The primary key of the database table</param>
		public static string SingleCacheKey<T>(this T obj) where T : class, ITableBacked, new() {
			return string.Format("{0}|one|{1}", obj.TableName, obj.PrimaryKey);
		}

		/// <summary>
		///		Gets the cache key for a query, where the condition might be
		///		"ColA=" and the param might be "123".
		/// </summary>
		/// <param name="condition">The SQL text following the WHERE</param>
		/// <param name="param">A value to be added to the WHERE Condition</param>
		public static string CacheKey<T>(this T obj, string condition, object param) where T : class, ITableBacked, new() {
			return string.Format("{0}|list|{1}{2}", obj.TableName, condition, param);
		}


		/// <summary>
		/// Gets the cache key for a whole SQL query, including its parameters (as passed in by an anonymous object)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static string SqlCacheKey<T>(this T obj, string sql, object ps) where T : class, ITableBacked, new() {
			return SqlCacheKey(obj.TableName, sql, ps);
		}


		public static string SqlCacheKey(string type, string sql, object ps) {
			string sqlParams = "none";
			if (ps != null) {
				sqlParams = string.Join("|", ps.GetType()
					.GetProperties()
					.Select(x => x.Name + "=" + (x.GetValue(ps) ?? "null").ToString())
				);
			}
			return string.Format("{0}|sql|{1}|{2}", type, sql, sqlParams);
		}



		public static string DumpJsonRowsCached(this DbConnection cn, ICacheProvider cache, string sql, object ps) {
			var cacheKey = SqlCacheKey("exp|custom_json", sql, ps);
			if (cache != null) {
				var cached = cache.Get<string>(cacheKey);
				if (cached != null) {
					return cached;
				}
			}
			string json = DatabaseConnector.DumpJsonRows(cn, sql, ps);

			if (cache != null) {
				Task.Run(() => {
					try {
						cache.Set<string>(cacheKey, json);
						cache.SetExpiry(cacheKey, new TimeSpan(0, 1, 0, 0));
					} catch { }
				});
			}
			return json;

		}

		//public static async Task<string> DumpJsonRowsCachedAsync(this DbConnection cn, ICacheProvider cache, string sql, object ps) {
		//	var cacheKey = SqlCacheKey("custom_json", sql, ps);
		//	if (cache != null) {
		//		var cached = await cache.GetAsync<string>(cacheKey);
		//		if (cached != null) {
		//			return cached;
		//		}
		//	}
		//	string json = await DatabaseConnector.DumpJsonRowsAsync(cn, sql, ps);

		//	if (cache != null) {
		//		await cache.SetAsync<string>(cacheKey, json);
		//	}

		//	return json;
		//}

		public static string DumpJsonRowsAndRefreshCached(this DbConnection cn, ICacheProvider cache, string sql, object ps) {
			var cacheKey = SqlCacheKey("exp|custom_json", sql, ps);

			string json = DatabaseConnector.DumpJsonRows(cn, sql, ps);

			if (cache != null) {
				Task.Run(() => {
					try {
						cache.Set<string>(cacheKey, json);
						cache.SetExpiry(cacheKey, new TimeSpan(0, 1, 0, 0));

					} catch { }
				});
			}
			return json;
		}

		//public static async Task<string> DumpJsonRowsAndRefreshCachedAsync(this DbConnection cn, ICacheProvider cache, string sql, object ps) {
		//	var cacheKey = SqlCacheKey("custom_json", sql, ps);

		//	string json = await DatabaseConnector.DumpJsonRowsAsync(cn, sql, ps);

		//	if (cache != null) {
		//		await cache.SetAsync<string>(cacheKey, json);

		//	}
		//	return json;
		//}



		//public static DataTable DumpDataTableCached(this DbConnection cn, ICacheProvider cache, string sql, object ps, TimeSpan expiry) {
		//	var cacheKey = SqlCacheKey("custom_datatable", sql, ps);
		//	if (cache != null) {
		//		var cached = cache.Get<DataTable>(cacheKey);
		//		if (cached != null) {
		//			return cached;
		//		}
		//	}
		//	DataTable tbl = DatabaseConnector.DumpDataTable(cn, sql, ps);
		//	if (cache != null) {
		//		Task.Run(() => {
		//			cache.Set<DataTable>(cacheKey, tbl);
		//			cache.SetExpiry(cacheKey, expiry);
		//		});
		//	}
		//	return tbl;
		//}
		public static DataTable DumpDataTableCached(this DbConnection cn, ICacheProvider cache, string sql, object ps, TimeSpan expires) {
			var cacheKey = SqlCacheKey("exp|custom_datatable", sql, ps);
			if (cache != null) {
				var cached = cache.Get<DataTable>(cacheKey);
				if (cached != null) {
					return cached;
				}
			}
			DataTable tbl = DatabaseConnector.DumpDataTable(cn, sql, ps);
			if (cache != null) {
				Task.Run(() => {
					try {
						cache.Set<DataTable>(cacheKey, tbl);
						cache.SetExpiry(cacheKey, expires);
					} catch { }
				});
			}
			return tbl;
		}

		//public static async Task<DataTable> DumpDataTableCachedAsync(this DbConnection cn, ICacheProvider cache, string sql, object ps) {
		//	var cacheKey = SqlCacheKey("custom_datatable", sql, ps);
		//	if (cache != null) {
		//		var cached = await cache.GetAsync<DataTable>(cacheKey);
		//		if (cached != null) {
		//			return cached;
		//		}
		//	}
		//	DataTable tbl = await DatabaseConnector.DumpDataTableAsync(cn, sql, ps);
		//	if (cache != null) {
		//		await cache.SetAsync<DataTable>(cacheKey, tbl);
		//	}
		//	return tbl;
		//}

		public static DataTable DumpDataTableAndRefreshCached(this DbConnection cn, ICacheProvider cache, string sql, object ps) {
			var cacheKey = SqlCacheKey("exp|custom_datatable", sql, ps);

			DataTable tbl = DatabaseConnector.DumpDataTable(cn, sql, ps);

			if (cache != null) {
				Task.Run(() => {
					try {
						cache.Set<DataTable>(cacheKey, tbl);
						cache.SetExpiry(cacheKey, new TimeSpan(0, 1, 0, 0));

					} catch { }
				});
			}
			return tbl;

		}

		//public static async Task<DataTable> DumpDataTableAndRefreshCachedAsync(this DbConnection cn, ICacheProvider cache, string sql, object ps) {
		//	var cacheKey = SqlCacheKey("custom_datatable", sql, ps);

		//	DataTable tbl = await DatabaseConnector.DumpDataTableAsync(cn, sql, ps);

		//	if (cache != null) {
		//		await cache.SetAsync<DataTable>(cacheKey, tbl);

		//	}
		//	return tbl;

		//}
	}
}
