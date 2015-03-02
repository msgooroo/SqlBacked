using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoGenerator {
	public class PocoColumn{

		public string ColumnName;
		public string DotNetType;
		public bool Nullable;
		public bool IsIdentity;
		public bool IsPrimaryKey;



		public PocoColumn(DbDataReader reader) {
			ColumnName = reader["COLUMN_NAME"].ToString();
			DotNetType = reader["DOT_NET_TYPE"].ToString();
			Nullable = ((string)reader["IS_NULLABLE"]) =="YES";
			IsIdentity = ((int)reader["IS_IDENTITY"]) == 1;
			IsPrimaryKey = ((int)reader["IS_PK"]) == 1;
		}

		public string PropertyCode {
			get {
				return string.Format("		public {0} {1};\r\n", DotNetType, ColumnName);
			}
		}


		public string BindReaderValueToObjectCode {
			get {
				if (DotNetType == "string" || DotNetType.Contains("?")) {
					if (DotNetType == "string") {
						return string.Format("			{0} = r[\"{0}\"] == DBNull.Value ? null : ({1}) r[\"{0}\"];", ColumnName, DotNetType);
					} else {
						return string.Format("			{0} = r[\"{0}\"] == DBNull.Value ? new {1}() : ({1}) r[\"{0}\"];", ColumnName, DotNetType);
					}
				} else {
					return string.Format("			{0} = ({1}) r[\"{0}\"];", ColumnName, DotNetType);
				}
			}
		}

		public string BindUpdateSql {
			get {
				return string.Format("{0} = @{0}", ColumnName);
			}
		}

		public string BindParameterToCommand{
			get {
				string value = "";
				if (DotNetType == "string") {
					value = string.Format("({0}==null ? (object) DBNull.Value : (object) {0})", ColumnName);
				} else if (Nullable) {
					value = string.Format("(!{0}.HasValue ? (object) DBNull.Value : (object) {0})", ColumnName);
				} else {
					value = string.Format("{0}", ColumnName);
				}
				return string.Format("cmd.Parameters.Add(DatabaseConnector.GetParameter(cmd, \"@{0}\", {1}));", ColumnName, value);
			}
		}
	}
}
