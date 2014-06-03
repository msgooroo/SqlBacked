

			public static IEnumerable<[{TYPE_NAME}]> Get(DbConnection cn, string condition, object param){
				return DatabaseConnector.Get<[{TYPE_NAME}]>(cn, condition, param);
			}

			public static [{TYPE_NAME}] Get(DbConnection cn, int primaryKey){
				return DatabaseConnector.Get<[{TYPE_NAME}]>(cn, primaryKey);
			}

			public static IEnumerable<[{TYPE_NAME}]> GetSql(DbConnection cn, string sql, object ps){
				return DatabaseConnector.GetSql<[{TYPE_NAME}]>(cn, sql, ps);
			}
