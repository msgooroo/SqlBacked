using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGooroo.SqlBacked {
	public interface ICacheProvider {

		void Set<T>(string cacheKey, T value) where T : class;
		T Get<T>(string cacheKey) where T : class;
		IEnumerable<T> GetMany<T>(IEnumerable<string> cacheKeys) where T : class;


		void Remove(string cacheKey);
	
	}
}
