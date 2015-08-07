using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace GoorooIO.SqlBacked {
	public static class DataReaderExtensions {


		public static List<Dictionary<string, object>> DumpReaderObject(this DbDataReader reader) {

			List<string> columnNames = Enumerable.Range(0, reader.FieldCount)
					.Select(x => reader.GetName(x))
					.ToList();

			var rows = new List<Dictionary<string, object>>();
			while (reader.Read()) {
				var row = new Dictionary<string, object>();
				for (var i = 0; i < columnNames.Count; i++) {
					row[columnNames[i]] = reader[i];
				}
				rows.Add(row);
			}

			return rows;


		}

	}
}
