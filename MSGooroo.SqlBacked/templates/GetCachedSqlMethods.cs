
	
			/// <summary>
			/// Executes the query given in the parameter 'sql' using the properties of 'ps' as parameters
			/// into the query.  Checks the cache first, returning the cached value if it exists, or failing that
			/// goes to the database, caching the value on the way through before returning it to you.
			/// </summary>
			/// <param name="cn">Database connection</param>
			/// <param name="cache">Cache to check prior to going to the database.</param>
			/// <param name="sql">SQL query to execute</param>
			/// <param name="param">Object to reflect to find parameters for the query</param>
			/// <returns></returns>
			public static IEnumerable<[{TYPE_NAME}]> GetSqlCached(DbConnection cn, ICacheProvider cache, string sql, object ps){
				return CacheConnector.GetSqlCached<[{TYPE_NAME}]>(cn, cache, sql, ps);
			}



	

			/// <summary>
			/// Executes the query given in the parameter 'sql' using the properties of 'ps' as parameters
			/// into the query.  Ignores any cached values, instead going directly to the database 
			/// updating / inserting the value into the cache before returning it to you.
			/// </summary>
			/// <param name="cn">Database connection</param>
			/// <param name="cache">Cache to check prior to going to the database.</param>
			/// <param name="sql">SQL query to execute</param>
			/// <param name="param">Object to reflect to find parameters for the query</param>
			/// <returns></returns>
			public static IEnumerable<[{TYPE_NAME}]> GetSqlAndRefreshCached(DbConnection cn, ICacheProvider cache, string sql, object ps){
				return CacheConnector.GetSqlAndRefreshCached<[{TYPE_NAME}]>(cn, cache, sql, ps);
			}

