using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoorooIO.SqlBacked {
	public delegate void DataRetrievedDelegate(List<ITableBacked> values);

	public class GetReference {
		public Action<List<ITableBacked>> Callback { get; set; }

		public DbCommand Command { get; set; }
		public string CacheKey { get; set; }
		public Type ResultType { get; set; }

		public bool ExpectSingleValue { get; set; }

		public List<ITableBacked> Result { get; set; }


		public GetReference(DbCommand cmd, string cacheKey, Type t, bool expectSingle, Action<List<ITableBacked>> callback) {
			Command = cmd;
			CacheKey = cacheKey;
			ResultType = t;
			ExpectSingleValue = expectSingle;
			Result = null;
			Callback = callback;
		}
	}

	public interface ICacheProvider {

		bool Set<T>(string cacheKey, T value) where T : class;
		//Task<bool> SetAsync<T>(string cacheKey, T value) where T : class;

		T Get<T>(string cacheKey) where T : class;
		//Task<T> GetAsync<T>(string cacheKey) where T : class;

		//List<T> GetMany<T>(List<string> cacheKeys) where T : class;
		//Task<List<T>> GetManyAsync<T>(List<string> cacheKeys) where T : class;

		void GetMany(List<GetReference> req);

		bool Remove(string cacheKey);
		//Task<bool> RemoveAsync(string cacheKey);

	}
}
