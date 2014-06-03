using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

using MSGooroo.TableBacked;

namespace [{NAMESPACE}] {

	[Serializable]
	public partial class [{TYPE_NAME}] : ITableBacked{

[{TABLE_NAME}]
[{SCHEMA_NAME}]
[{PRIMARY_KEY}]

#region Properties
[{PROPERTIES}]
#endregion

#region SQL commands
[{INSERT_SQL}]

[{UPDATE_SQL}]
#endregion

#region Database binding
[{BIND_READER}]

[{BIND_COMMAND}]
#endregion

#region Get Methods
[{GET_METHODS}]

[{GET_CACHE_METHODS}]


#endregion
	}
}