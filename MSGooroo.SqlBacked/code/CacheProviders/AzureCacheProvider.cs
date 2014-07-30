using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.ApplicationServer.Caching;


namespace MSGooroo.SqlBacked {
	public class AzureCacheProvider : ICacheProvider {
		DataCache _cache;

		public AzureCacheProvider(DataCache cache) {
			_cache = cache;
		}

		#region ICacheProvider Members

		public void Set<T>(string cacheKey, T value) where T : class {
			if (_cache == null) {
				return;
			}
			//try {
			_cache.Put(cacheKey, value);
			//} catch {
			//	return;
			//}

		}

		public T Get<T>(string cacheKey) where T : class {
			if (_cache == null) {
				return null;
			}
			//try {
			return _cache.Get(cacheKey) as T;
			//} catch {
			//	return null;
			//}
		}


		public void Remove(string cacheKey) {
			if (_cache == null) {
				return;
			}
			//try {
			_cache.Remove(cacheKey);
			//} catch {

			//	return;
			//}
		}

		public IEnumerable<T> GetMany<T>(IEnumerable<string> cacheKeys) where T : class {
			if (_cache == null) {
				return null;
			}
			var items = _cache.BulkGet(cacheKeys);
			//try {
			return items.Select(x => x.Value as T);
			//} catch {
			//	return null;
			//}
		}

		#endregion





		public void Flush() {
			if (_cache == null) {
				return;
			}
			_cache.Clear();
		}
	}
}
