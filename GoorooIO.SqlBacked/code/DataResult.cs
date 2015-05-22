using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GoorooIO.SqlBacked {
    using RowData = Dictionary<string, string>;

    public class DataResult {

        public List<string> ColumnNames;
        public List<RowData> Rows;

        public DataResult(DbDataReader reader) {
            ColumnNames =
                Enumerable.Range(0, reader.FieldCount)
                    .Select(x => reader.GetName(x))
                    .ToList();
            Rows = new List<RowData>();
            while (reader.Read()) {
                Dictionary<string, string> rowData = new Dictionary<string, string>();
                for (var i = 0; i < reader.FieldCount; i++) {
                    if (reader[i].GetType() == typeof(DateTime)) {
                        // Use ISO time
                        rowData[ColumnNames[i]] = ((DateTime)reader[i]).ToString("s");
                    } else {
                        rowData[ColumnNames[i]] = reader[i].ToString();
                    }
                }
                Rows.Add(rowData);
            }

        }

        // For deserializing
        public DataResult() { }

        public DataResult(string json) {
            var result = JsonConvert.DeserializeObject<DataResult>(json);

            ColumnNames = result.ColumnNames;
            Rows = result.Rows;

        }



        public string GetJson() {
            return JsonConvert.SerializeObject(this);
        }
    }
}
