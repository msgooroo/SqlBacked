using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Threading;
using System.Data.Common;

using GoorooIO.SqlBacked;

namespace GoorooIO.SqlBacked {
	public class EntityCache<T> where T : class, ITableBacked, new() {


		public int Timeout = 100;

		private bool _loaded = false;
		private bool _loading = false;

		private string _where;
		private string _order;

		private Func<DbConnection> _fnGetCn;


		private List<T> _objects = new List<T>();

		public bool IsLoaded { get { return _loaded; } }
		public bool IsLoading { get { return _loading; } }

		public EntityCache(Func<DbConnection> fnGetCn) {
			_fnGetCn = fnGetCn;
			Load();
		}

		public EntityCache() {
			Load();
		}
		public EntityCache(string where, string order) {
			if (!string.IsNullOrEmpty(where)) {
				_where = "WHERE " + where;
			}
			if (!string.IsNullOrEmpty(order)) {
				_order = "ORDER BY " + order;
			}
			Load();
		}

		private void Initialize() {
			Task.Run(() => {
				// Add some jitter to stop things all happening at once.
				//Thread.Sleep((int)(new Random().NextDouble() * 1000));
				Load();
			});
		}

		public List<T> Cached {
			get {

				if (!_loaded) {
					throw new InvalidOperationException("Cache not loaded, please check .IsLoaded field before accessing .Cached");
				}
				return _objects;

			}
		}

		public bool WaitReady() {
			int count = 0;
			while (!_loaded && count < Timeout) {
				Thread.Sleep(1);
				count++;
			}
			return count < Timeout;
		}

		private void Load() {
			//if (Monitor.TryEnter(_locker, 30000)) {
			// Might have been waiting at the lock
			try {

				// If you are loaded, then dont do it again
				if (_loaded || _loading) {
					return;
				}
				_loading = true;

				using (var cn = _fnGetCn()) {

					IEnumerable<T> query = null;
					if (_where != null || _order != null) {
						string sql = string.Format(@"SELECT * FROM {0} {1} {2}",
							new T().SchemaName + "." + new T().TableName, _where, _order);
						query = DatabaseConnector.GetSql<T>(cn, sql, null).ToList();
					} else {
						query = DatabaseConnector.Get<T>(cn, "1=", 1).ToList();
					}

					foreach (var l in query) {
						_objects.Add(l);
						//_objectsById[l.PrimaryKey] = l;
					}

				}

				_loaded = true;
			} finally {

				_loading = false;
				//Monitor.Exit(_locker);
			}
			//}
		}


		//public T Get(int id) {

		//	if (!_loaded) {
		//		throw new InvalidOperationException("Cache not loaded, please check .IsLoaded field before accessing .Get()");

		//	}
		//	// Use the memory cache
		//	T obj = null;
		//	if (_objectsById.TryGetValue(id, out obj)) {
		//		return obj;
		//	} else {
		//		return null;
		//	}
		//}


	}
}
