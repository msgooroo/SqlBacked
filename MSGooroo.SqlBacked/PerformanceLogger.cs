using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Collections;
using System.Diagnostics;

namespace MSGooroo.SqlBacked {
	public class PerformanceLogger : IDisposable {

		public class Counter {
			public double TotalMilliseconds;
			public int Count;
			public string Stack;

			public void Add(PerformanceLogger log) {
				Count++;
				TotalMilliseconds += log._time;


				Stack = Environment.StackTrace;

			}
		}

		public static double TotalTime(string counterName, IDictionary persistence) {
			Counter counter;
			var counters = PerformanceCounters(persistence);

			if (counters != null && counters.TryGetValue(counterName, out counter)) {
				return counter.TotalMilliseconds;
			} else {
				return 0;
			}
		}
		public static int Count(string counterName, IDictionary persistence) {
			Counter counter;
			var counters = PerformanceCounters(persistence);

			if (counters != null && counters.TryGetValue(counterName, out counter)) {
				return counter.Count;
			} else {
				return 0;
			}
		}

		public static Dictionary<string, Counter> PerformanceCounters(IDictionary persistence) {
			if (persistence != null) {
				return persistence["request-counters"] as Dictionary<string, Counter>;
			} else {
				return null;
			}
		}
		private void Increment(PerformanceLogger logger) {
			if (_persistence != null) {
				if (_persistence != null) {
					Dictionary<string, Counter> counters;
					if (_persistence.Contains("request-counters")) {
						counters = _persistence["request-counters"] as Dictionary<string, Counter>;
					} else {
						counters = new Dictionary<string, Counter>();
						_persistence["request-counters"] = counters;
					}

					if (counters.ContainsKey(logger._name)) {
						counters[logger._name].Add(logger);
					} else {
						counters[logger._name] = new Counter() {
							TotalMilliseconds = logger._time,
							Count = 1,
							Stack = Environment.StackTrace
						};
					}
				}

			}

		}


		private DateTime _startTime;
		private double _time;
		private string _name;
		private IDictionary _persistence;


		public PerformanceLogger(string name, System.Collections.IDictionary persistence) {
			_persistence = persistence;
			_startTime = DateTime.UtcNow;
			_name = name;

		}

		public void Dispose() {
			_time = DateTime.UtcNow.Subtract(_startTime).TotalMilliseconds;
			Increment(this);


			System.Diagnostics.Trace.WriteLine(string.Format("log\t{0}: {1}ms", _name, _time));
		}
	}
}
