		[JsonIgnore]
		public string UpdateSql  { 
			get {
				return @"UPDATE [{SCHEMA_NAME}].[{TYPE_NAME}] SET
							[{UPDATE_SQL}]
						WHERE [{PKCOLUMN}] = @PkColumn
				";
			}
		}
