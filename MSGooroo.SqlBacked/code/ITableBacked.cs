using System;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGooroo.SqlBacked {
	public interface ITableBacked {
		void BindReader(DbDataReader reader);
		void BindCommand(DbCommand command);

		string TableName { get; }
		string SchemaName { get; }
		string PrimaryKeyColumn { get; }

		string InsertSql { get; }
		string UpdateSql { get; }

		int PrimaryKey { get; }



	}
}
