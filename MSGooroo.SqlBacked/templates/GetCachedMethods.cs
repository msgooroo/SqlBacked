

			public static IEnumerable<[{TYPE_NAME}]> GetCached(DbConnection cn, ICacheProvider cache, string condition, object param){
				return CacheConnector.GetCached<[{TYPE_NAME}]>(cn, cache, condition, param);
			}

			public static [{TYPE_NAME}] GetCached(DbConnection cn, ICacheProvider cache, int primaryKey){
				return CacheConnector.GetCached<[{TYPE_NAME}]>(cn, cache, primaryKey);
			}

			public static IEnumerable<[{TYPE_NAME}]> GetSqlCached(DbConnection cn, ICacheProvider cache, string sql, object ps){
				return CacheConnector.GetSqlCached<[{TYPE_NAME}]>(cn, cache, sql, ps);
			}
