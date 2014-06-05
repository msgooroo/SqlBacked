

			public static IEnumerable<[{TYPE_NAME}]> GetCached(DbConnection cn, ICacheProvider cache, string condition, object param){
				return CacheConnector.GetCached<[{TYPE_NAME}]>(cn, cache, condition, param);
			}

			public static [{TYPE_NAME}] GetCached(DbConnection cn, ICacheProvider cache, int primaryKey){
				return CacheConnector.GetCached<[{TYPE_NAME}]>(cn, cache, primaryKey);
			}

	
			/// <summary>
			/// Get the items from the database, using the WHERE condition supplied with condition and the suppliec parameter,
			/// and refresh the cache asynchronously
			/// </summary>
			/// <param name="cn"></param>
			/// <param name="cache"></param>
			/// <param name="condition"></param>
			/// <param name="param"></param>
			/// <returns></returns>
			public static IEnumerable<[{TYPE_NAME}]> GetAndRefreshCached(DbConnection cn, ICacheProvider cache, string condition, object param){
				return CacheConnector.GetAndRefreshCached<[{TYPE_NAME}]>(cn, cache, condition, param);
			}

			/// <summary>
			/// Get the item from the database, and refresh the cache asynchronously
			/// </summary>
			/// <param name="cn"></param>
			/// <param name="cache"></param>
			/// <param name="condition"></param>
			/// <param name="param"></param>
			/// <returns></returns>
			public static [{TYPE_NAME}] GetAndRefreshCached(DbConnection cn, ICacheProvider cache, int primaryKey){
				return CacheConnector.GetAndRefreshCached<[{TYPE_NAME}]>(cn, cache, primaryKey);
			}
