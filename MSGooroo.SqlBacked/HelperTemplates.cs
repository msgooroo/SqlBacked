
			public static DbParameter GetParameter(DbCommand cmd, string name, object val){
				DbParameter p = cmd.CreateParameter();
				p.ParameterName = name;
				p.Value = val == null ? (object) DBNull.Value : val;
				return p;
			}