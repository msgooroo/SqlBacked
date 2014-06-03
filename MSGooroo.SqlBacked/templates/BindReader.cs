
		/// <summary>
		///		Populates the properties of the object from the reader
		/// </summary>
		/// <param name="cmd">The open and active reader to get vales from</param>
		public void BindReader(DbDataReader r){
			

[{READER_BINDINGS}]

[{PRIMARY_KEY_BINDING}]
		}


		/// <summary>
		///		Creates a new object from the values of the reader.
		/// </summary>
		/// <param name="cmd">The open and active reader to get vales from</param>
		public static [{TYPE_NAME}] Populate(DbDataReader r){
			[{TYPE_NAME}] x = new [{TYPE_NAME}]();
			x.BindReader(r);
			return x;
		}