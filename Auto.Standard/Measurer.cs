using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Auto
{
    public class Measurer
    {
        private readonly List<(string, TimeSpan)> _measureValues = new();
        private readonly Logger                   _logger;

        public struct MeasureBit : IDisposable
        {
            private string                   _msg;
            private Action<string, TimeSpan> _log;
            private Stopwatch                _stopwatch;

            public MeasureBit(string msg, Action<string, TimeSpan> log)
            {
                _msg = msg;
                _log = log;
                _stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                if(_stopwatch == null) return;
                _stopwatch.Stop();
                _log(_msg, _stopwatch.Elapsed);
                _msg = null;
                _log = null;
                _stopwatch = null;
            }
        }

        public Measurer(Logger logger)
        {
            _logger = logger;
        }

        public void StartMeasure()
        {
            _measureValues.Clear();
        }

        public MeasureBit Measure(Action action)
        {
            using var measurer = Measure(action.Method.Name);
            action();
            return measurer;
        }

        public MeasureBit Measure(string prefix, Action action)
        {
            using var measurer = Measure(prefix + " " + action.Method.Name);
            action();
            return measurer;
        }

        public MeasureBit Measure(string msg)
        {
            _logger.Info?.Invoke("\n== started " + msg);

            var measurer = new MeasureBit(msg,
                (msg, span) =>
                {
                    _logger.Info?.Invoke($"== finished {msg} in {span.TotalSeconds} sec");
                    _measureValues.Add((msg, span));
                    _logger.Info?.Invoke("\n");
                });

            return measurer;
        }

        public void EndMeasure()
        {
            _logger.Info?.Invoke("=============Total=======\n");
            var totalTime = TimeSpan.Zero;
            foreach((string msg, TimeSpan time) in _measureValues)
            {
                _logger.Info?.Invoke($"== finished {msg} in {(int)time.TotalMilliseconds} ms");
                totalTime += time;
            }
            _logger.Info?.Invoke($"\n=============Finished in {(int)totalTime.TotalMilliseconds} ms=======\n");
        }
    }
}