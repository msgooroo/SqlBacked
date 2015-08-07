using System;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace GoorooIO.SqlBacked {
	public abstract class ReadOnlyTableBacked : ITableBacked {
		public abstract void BindReader(DbDataReader reader);

		public void BindCommand(System.Data.Common.DbCommand command) {
			throw new NotImplementedException();
		}

		[JsonIgnore]
		[ScriptIgnore]
		public string TableName {
			get { throw new NotImplementedException(); }
		}

		[JsonIgnore]
		[ScriptIgnore]
		public string SchemaName {
			get { throw new NotImplementedException(); }
		}

		[JsonIgnore]
		[ScriptIgnore]
		public string PrimaryKeyColumn {
			get { throw new NotImplementedException(); }
		}

		[JsonIgnore]
		[ScriptIgnore]
		public string InsertSql {
			get { throw new NotImplementedException(); }
		}

		[JsonIgnore]
		[ScriptIgnore]
		public string UpdateSql {
			get { throw new NotImplementedException(); }
		}

		[JsonIgnore]
		[ScriptIgnore]
		public int PrimaryKey {
			get { throw new NotImplementedException(); }
		}



	}
}
