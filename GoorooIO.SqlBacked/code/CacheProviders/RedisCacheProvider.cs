using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

using StackExchange.Redis;


namespace GoorooIO.SqlBacked {
	public class RedisCacheProvider : ICacheProvider {

		private IDatabase _db;
		private IDictionary _loggingPersistence;

		public RedisCacheProvider(IDatabase db, IDictionary loggingPersistence) {
			_db = db;
			_loggingPersistence = loggingPersistence;
		}

		public void GetMany(List<GetReference> req) {
			if (_db == null) {
				return;
			}

			using (var log = new PerformanceLogger("redis-get-many", _loggingPersistence)) {
				
				var values = _db.StringGet(
						req.Select(x => (RedisKey)x.CacheKey)
							.ToArray()
				);

				for (var i = 0; i < values.Length; i++) {
					if (req[i].ExpectSingleValue) {
						var val = DeserializeSingle(values[i]);
						if (val == null) {
							req[i].Result = null;
						} else {
							req[i].Result = new ITableBacked[] { val }.ToList();
						}
					} else {
						req[i].Result = DeserializeList(values[i]);
					}
				}
			}
			return;
		}


		#region ICacheProvider Members

		public bool Set<T>(string cacheKey, T value) where T : class {
			if (_db == null) {
				return false;
			}
			using (var log = new PerformanceLogger("redis-set", _loggingPersistence)) {
				using (var log2 = new PerformanceLogger("redis-set|" + cacheKey, _loggingPersistence)) {
					byte[] buffer = Serialize(value);
					_db.StringSet(cacheKey, buffer);

					return true;
				}
			}

		}

		public T Get<T>(string cacheKey) where T : class {
			if (_db == null) {
				return null;
			}


			using (var log = new PerformanceLogger("redis-get", _loggingPersistence)) {
				using (var log2 = new PerformanceLogger("redis-get|" + cacheKey, _loggingPersistence)) {
					byte[] buffer = _db.StringGet(cacheKey);
					return Deserialize<T>(buffer);
				}
			}
		}


		public bool Remove(string cacheKey) {
			if (_db == null) {
				return false;
			}
			_db.KeyDelete(cacheKey);

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


		#region ICacheProvider Members Async versions

		public async Task<bool> SetAsync<T>(string cacheKey, T value) where T : class {
			if (_db == null) {
				return false;
			}

			using (var log = new PerformanceLogger("redis-set", _loggingPersistence)) {
				using (var log2 = new PerformanceLogger("redis-set|" + cacheKey, _loggingPersistence)) {
					byte[] buffer = Serialize(value);
					return await _db.StringSetAsync(cacheKey, buffer);
				}
			}

		}

		public async Task<T> GetAsync<T>(string cacheKey) where T : class {
			if (_db == null) {
				return null;
			}


			using (var log = new PerformanceLogger("redis-get", _loggingPersistence)) {
				using (var log2 = new PerformanceLogger("redis-get|" + cacheKey, _loggingPersistence)) {
					byte[] buffer = await _db.StringGetAsync(cacheKey);
					return Deserialize<T>(buffer);
				}
			}
		}


		public async Task<bool> RemoveAsync(string cacheKey) {
			if (_db == null) {
				return false;
			}
			return await _db.KeyDeleteAsync(cacheKey);

		}

		public async Task<List<T>> GetManyAsync<T>(List<string> cacheKeys) where T : class {
			if (_db == null) {
				return null;
			}
			using (var log = new PerformanceLogger("redis-get-many", _loggingPersistence)) {
				var values = await _db.StringGetAsync(
						cacheKeys
							.Select(x => (RedisKey)x)
							.ToArray()
				);
				return values.Select(x => Deserialize<T>(x)).ToList();
			}
		}

		#endregion


		static byte[] Serialize<T>(T o) {
			if (o == null) {
				return null;
			}

			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using (MemoryStream memoryStream = new MemoryStream()) {
				binaryFormatter.Serialize(memoryStream, o);
				byte[] objectDataAsStream = memoryStream.ToArray();
				return objectDataAsStream;
			}
		}

		static T Deserialize<T>(byte[] stream) {
			if (stream == null) {
				return default(T);
			}

			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using (MemoryStream memoryStream = new MemoryStream(stream)) {
				T result = (T)binaryFormatter.Deserialize(memoryStream);
				return result;
			}
		}

		static ITableBacked DeserializeSingle(byte[] stream) {
			if (stream == null) {
				return null;
			}

			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using (MemoryStream memoryStream = new MemoryStream(stream)) {
				ITableBacked result = (ITableBacked)binaryFormatter.Deserialize(memoryStream);
				return result;
			}
		}

		static List<ITableBacked> DeserializeList(byte[] stream) {
			if (stream == null) {
				return null;
			}

			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using (MemoryStream memoryStream = new MemoryStream(stream)) {
				List<ITableBacked> result = ((IEnumerable)binaryFormatter.Deserialize(memoryStream))
					.Cast<ITableBacked>()
					.ToList();
				return result;
			}
		}

	}
}
