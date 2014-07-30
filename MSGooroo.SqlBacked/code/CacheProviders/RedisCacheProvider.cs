﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.Threading;
using System.Threading.Tasks;

using StackExchange.Redis;


namespace MSGooroo.SqlBacked {
	public class RedisCacheProvider : ICacheProvider {

		private IDatabase _db;
		private IServer _server;
		public RedisCacheProvider(IDatabase db) {
			_db = db;
		}


		public void SetAdminConnection(ConnectionMultiplexer cn) {

			var endpoints = cn.GetEndPoints();
			_server = cn.GetServer(endpoints.First());
		}
		public void Flush() {
			if (_server == null) {
				throw new InvalidOperationException("Please call .SetAdminConnection() to set the server connection");
			}

			_server.FlushAllDatabases();
		}

		#region ICacheProvider Members

		public void Set<T>(string cacheKey, T value) where T : class {
			if (_db == null) {
				return;
			}
			byte[] buffer = Serialize(value);
			_db.StringSet(cacheKey, buffer);

		}

		public T Get<T>(string cacheKey) where T : class {
			if (_db == null) {
				return null;
			}

			byte[] buffer = _db.StringGet(cacheKey);
			return Deserialize<T>(buffer);

		}


		public void Remove(string cacheKey) {
			if (_db == null) {
				return;
			}
			_db.KeyDelete(cacheKey);

		}

		public IEnumerable<T> GetMany<T>(IEnumerable<string> cacheKeys) where T : class {
			if (_db == null) {
				return null;
			}
			var values = _db.StringGet(
					cacheKeys
						.Select(x => (RedisKey)x)
						.ToArray()
			);
			return values.Select(x => Deserialize<T>(x)).ToList();

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


	}
}
