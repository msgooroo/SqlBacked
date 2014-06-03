using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGooroo.TableBacked {
	public interface ICacheProvider {

		void Set<T>(string cacheKey, T value);
		T Get<T>(string cacheKey);
		IEnumerable<T> GetMany<T>(IEnumerable<string> cacheKeys);


		void Remove(string cacheKey);
	
	}
}
