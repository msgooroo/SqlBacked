using System;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Reflection;

namespace GoorooIO.SqlBacked {

	public class BatchContext {

		private List<Action> _completeCallbacks;
		private List<GetReference> _requests;
		private DbConnection _db;
		private ICacheProvider _cache;


		public BatchContext(DbConnection db, ICacheProvider cache) {
			_db = db;
			_cache = cache;
			_requests = new List<GetReference>();
			_completeCallbacks = new List<Action>();
		}

		public void OnComplete(Action action) {
			_completeCallbacks.Add(action);
		}



		public void ScheduleTable<T>(string sql, object ps, Action<DataTable> callback) where T : class, ITableBacked, new() {


		}

		public void ScheduleSql<T>(string sql, object ps, Action<List<ITableBacked>> callback) where T : class, ITableBacked, new() {
			T first = new T();
			string cacheKey = first.SqlCacheKey(sql, ps);
			DbCommand cmd = _db.GetSqlCommand<T>(sql, ps);

			_requests.Add(new GetReference(cmd, cacheKey, typeof(T), false, callback));
		}

		public void Schedule<T>(int primaryKey, Action<List<ITableBacked>> callback) where T : class, ITableBacked, new() {
			T first = new T();
			string cacheKey = first.SingleCacheKey(primaryKey);
			DbCommand cmd = _db.GetCommand<T>(primaryKey);

			_requests.Add(new GetReference(cmd, cacheKey, typeof(T), false, callback));
		}

		public void Schedule<T>(string condition, object ps, Action<List<ITableBacked>> callback) where T : class, ITableBacked, new() {
			T first = new T();
			string cacheKey = first.CacheKey<T>(condition, ps);
			DbCommand cmd = _db.GetCommand<T>(condition, ps);

			_requests.Add(new GetReference(cmd, cacheKey, typeof(T), false, callback));
		}

		public void Execute() {
			_cache.GetMany(_requests);

			var needUpdating = new List<GetReference>();
			var uncasted = new List<object>();

			foreach (var r in _requests) {
				if (r.Result == null) {
					// Missed it in the cache, so do it in the database...
					if (r.ExpectSingleValue) {
						MethodInfo method = typeof(DatabaseConnector).GetMethod("GetSingle");
						MethodInfo genericMethod = method.MakeGenericMethod(r.ResultType);
						var raw = genericMethod.Invoke(null, new object[] { r.Command });
						var result = (ITableBacked)raw;
						r.Result = new ITableBacked[] { result }.ToList();

						needUpdating.Add(r);
						uncasted.Add(raw);
					} else {
						MethodInfo method = typeof(DatabaseConnector).GetMethod("GetList");
						MethodInfo genericMethod = method.MakeGenericMethod(r.ResultType);
						var raw =  genericMethod.Invoke(null, new object[] { r.Command });
						
						r.Result = ((IEnumerable) raw).Cast<ITableBacked>().ToList();

						needUpdating.Add(r);
						uncasted.Add(raw);
					}

				}

				r.Callback(r.Result);
			}

			foreach(var cb in _completeCallbacks){
				cb();
			}

			Task.Run(() => {
				for (int i = 0; i < needUpdating.Count; i++ ) {
					var r = needUpdating[i];
					MethodInfo method = typeof(ICacheProvider).GetMethod("Set");

					if (needUpdating[i].ExpectSingleValue) {
						MethodInfo genericMethod = method.MakeGenericMethod(r.ResultType);
						var result = (bool)genericMethod.Invoke(_cache, new object[] { r.CacheKey, uncasted[i] });

					} else {
						Type generic = typeof(List<>);
						Type listish = generic.MakeGenericType(r.ResultType);
						MethodInfo genericMethod = method.MakeGenericMethod(listish);
						var result = (bool)genericMethod.Invoke(_cache, new object[] { r.CacheKey, uncasted[i] });
					}

				}

			});

		}




	}
}
