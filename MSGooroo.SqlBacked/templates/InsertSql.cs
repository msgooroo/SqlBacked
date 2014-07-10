		[JsonIgnore]
		public string InsertSql { 
			get {
				return  @"INSERT INTO [{SCHEMA_NAME}].[{TYPE_NAME}] ([{COLUMN_LIST}])
							SELECT [{VALUE_LIST}]
						SELECT SCOPE_IDENTITY()";
			}
		}
