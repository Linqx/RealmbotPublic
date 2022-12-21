using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Bookkeeping {
    public class Benchmarker {
        private readonly Dictionary<string, Action> tests = new Dictionary<string, Action>();

        public void AddTest(string name, Action method) {
            tests.Add(name, method);
        }

        public void RunTests() {
            Stopwatch watch = new Stopwatch();
            Dictionary<string, long> timings = new Dictionary<string, long>();
            foreach (KeyValuePair<string, Action> test in tests) {
                watch.Restart();
                test.Value.Invoke();
                watch.Stop();

                timings.Add(test.Key, watch.ElapsedTicks);
            }

            IOrderedEnumerable<KeyValuePair<string, long>> timingList = timings.OrderBy(_ => _.Value);

            LogEntry entry = new LogEntry();
            long first = -1;
            foreach (KeyValuePair<string, long> timing in timingList) {
                StringBuilder builder = new StringBuilder();
                if (timing.Value / TimeSpan.TicksPerMillisecond > 10)
                    builder.Append($"{timing.Key} time in MS: {timing.Value / TimeSpan.TicksPerMillisecond}");
                else
                    builder.Append($"{timing.Key} time in TICKS: {timing.Value}");

                if (first != -1) {
                    long dif = timing.Value - first;
                    builder.Append($" | {(int) (dif / (double) timing.Value * 100)}% slower");
                }
                else {
                    first = timing.Value;
                }

                builder.Append('\n');
                entry.Append(builder.ToString());
            }

            Log.Write(entry);
        }
    }
}