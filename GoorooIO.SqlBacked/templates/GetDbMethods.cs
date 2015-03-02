

			public static List<[{TYPE_NAME}]> Get(DbConnection cn, string condition, object param){
				return DatabaseConnector.Get<[{TYPE_NAME}]>(cn, condition, param);
			}

			public static [{TYPE_NAME}] Get(DbConnection cn, int primaryKey){
				return DatabaseConnector.Get<[{TYPE_NAME}]>(cn, primaryKey);
			}

			public static List<[{TYPE_NAME}]> GetSql(DbConnection cn, string sql, object ps){
				return DatabaseConnector.GetSql<[{TYPE_NAME}]>(cn, sql, ps);
			}





			//public static async Task<List<[{TYPE_NAME}]>> GetAsync(DbConnection cn, string condition, object param){
			//	return await DatabaseConnector.GetAsync<[{TYPE_NAME}]>(cn, condition, param);
			//}

			//public static async Task<[{TYPE_NAME}]> GetAsync(DbConnection cn, int primaryKey){
			//	return await DatabaseConnector.GetAsync<[{TYPE_NAME}]>(cn, primaryKey);
			//}

			//public static async Task<List<[{TYPE_NAME}]>> GetSqlAsync(DbConnection cn, string sql, object ps){
			//	return await DatabaseConnector.GetSqlAsync<[{TYPE_NAME}]>(cn, sql, ps);
			//}
