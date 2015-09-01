using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Caching;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

using StackExchange.Redis;


namespace GoorooIO.SqlBacked {
	public class MemoryCacheProvider : ICacheProvider {

		private MemoryCache _cache;
		private IDictionary _loggingPersistence;

		public MemoryCacheProvider(MemoryCache cache, IDictionary loggingPersistence) {
			_cache = cache;
			_loggingPersistence = loggingPersistence;
		}

		public void GetMany(List<GetReference> req) {
			if (_cache == null) {
				return;
			}

			using (var log = new PerformanceLogger("redis-get-many", _loggingPersistence)) {
				for (var i = 0; i < req.Count; i++) {
					var r = req[i];
					if (r.ExpectSingleValue) {
						var val = _cache.Get(r.CacheKey) as ITableBacked;

						if (val == null) {
							req[i].Result = null;
						} else {
							req[i].Result = new ITableBacked[] { val }.ToList();
						}
					} else {
						req[i].Result = _cache.Get(r.CacheKey) as List<ITableBacked>;
					}
				}
			}
			return;
		}


		#region ICacheProvider Members

		public bool Set<T>(string cacheKey, T value) where T : class {
			if (_cache == null) {
				return false;
			}
			using (var log = new PerformanceLogger("redis-set", _loggingPersistence)) {
				using (var log2 = new PerformanceLogger("redis-set|" + cacheKey, _loggingPersistence)) {
					_cache.Set(cacheKey, value, new CacheItemPolicy());
					return true;
				}
			}

		}

		public T Get<T>(string cacheKey) where T : class {
			if (_cache == null) {
				return null;
			}


			using (var log = new PerformanceLogger("redis-get", _loggingPersistence)) {
				using (var log2 = new PerformanceLogger("redis-get|" + cacheKey, _loggingPersistence)) {
					return _cache.Get(cacheKey) as T;
				}
			}
		}


		public bool Remove(string cacheKey) {
			if (_cache == null) {
				return false;
			}
			_cache.Remove(cacheKey);

			return true;
		}

		//public List<T> GetMany<T>(List<string> cacheKeys) where T : class {
		//	if (_db == null) {
		//		return null;
		//	}
		//	using (var log = new PerformanceLogger("redis-get-many", _loggingPersistence)) {
		//		var values = _db.StringGet(
		//				cacheKeys
		//					.Select(x => (RedisKey)x)
		//					.ToArray()
		//		);
		//		return values.Select(x => Deserialize<T>(x)).ToList();
		//	}
		//}

		#endregion




		public bool SetExpiry(string cacheKey, TimeSpan timeout) {
			throw new NotImplementedException();
		}
	}
}
