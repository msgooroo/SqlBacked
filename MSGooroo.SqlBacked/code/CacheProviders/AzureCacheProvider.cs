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
			_cache.Put(cacheKey, value);
			

		}

		public T Get<T>(string cacheKey) where T: class {
			if (_cache == null) {
				return null;
			}
			return _cache.Get(cacheKey) as T;

		}


		public void Remove(string cacheKey) {
			if (_cache == null) {
				return;
			}
			_cache.Remove(cacheKey);

		}

		public IEnumerable<T> GetMany<T>(IEnumerable<string> cacheKeys) where T : class {
			if (_cache == null) {
				return null;
			}
			var items = _cache.BulkGet(cacheKeys);
			return items.Select(x => x.Value as T);

		}

		#endregion



	}
}
