using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MSGooroo.SqlBacked {
	public class PerformanceLogger : IDisposable {

		public class Counter {
			public double TotalMilliseconds;
			public int Count; 

			public void Add(PerformanceLogger log){
				Count++;
				TotalMilliseconds += log._time;
			}
		}

		public static double TotalTime(string counterName) {
			Counter counter;
			var counters = PerformanceCounters;

			if (counters != null && counters.TryGetValue(counterName, out counter)) {
				return counter.TotalMilliseconds;
			} else {
				return 0;
			}
		}
		public static int Count(string counterName) {
			Counter counter;
			var counters = PerformanceCounters;

			if (counters != null && counters.TryGetValue(counterName, out counter)) {
				return counter.Count;
			} else {
				return 0;
			}
		}

		public static Dictionary<string, Counter> PerformanceCounters {
			get {
				return HttpContext.Current.Items["request-counters"] as Dictionary<string, Counter>;
			}
		}
		private static void Increment(PerformanceLogger logger) {
			if (HttpContext.Current != null) {
				if (HttpContext.Current.Items != null) {
					Dictionary<string, Counter> counters;
					if (HttpContext.Current.Items.Contains("request-counters")) {
						counters = HttpContext.Current.Items["request-counters"] as Dictionary<string, Counter>;
					} else {
						counters = new Dictionary<string, Counter>();
						HttpContext.Current.Items["request-counters"] = counters;
					}

					if (counters.ContainsKey(logger._name)) {
						counters[logger._name].Add(logger);
					} else {
						counters[logger._name] = new Counter() { 
							TotalMilliseconds = logger._time, 
							Count = 1 };
					}
				}

			}

		}


		private DateTime _startTime;
		private double _time;
		private string _name;
		public PerformanceLogger(string name) {
			_startTime = DateTime.UtcNow;
			_name = name;

		}

		public void Dispose() {
			_time = DateTime.UtcNow.Subtract(_startTime).TotalMilliseconds;
			Increment(this);
		}
	}
}
