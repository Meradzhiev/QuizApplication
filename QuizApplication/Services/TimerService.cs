using System.Collections.Concurrent;

namespace QuizApplication.Services
{
    public class TimerService : ITimerService, IDisposable
    {
        private readonly ConcurrentDictionary<string, Timer> _timers;
        private readonly ConcurrentDictionary<string, DateTime> _endTimes;

        public event Action<string> OnTimerElapsed;

        public TimerService()
        {
            _timers = new ConcurrentDictionary<string, Timer>();
            _endTimes = new ConcurrentDictionary<string, DateTime>();
        }

        public void StartTimer(string sessionId, TimeSpan duration)
        {
            StopTimer(sessionId); // Stop existing timer if any

            var endTime = DateTime.Now.Add(duration);
            _endTimes[sessionId] = endTime;

            var timer = new Timer(_ =>
            {
                TimerCallback(sessionId);
            }, null, duration, Timeout.InfiniteTimeSpan);

            _timers[sessionId] = timer;
        }

        public void StopTimer(string sessionId)
        {
            if (_timers.TryRemove(sessionId, out var timer))
            {
                timer?.Dispose();
            }
            _endTimes.TryRemove(sessionId, out _);
        }

        public TimeSpan GetRemainingTime(string sessionId)
        {
            if (_endTimes.TryGetValue(sessionId, out var endTime))
            {
                var remaining = endTime - DateTime.Now;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
            return TimeSpan.Zero;
        }

        public bool IsTimerRunning(string sessionId)
        {
            return _endTimes.ContainsKey(sessionId);
        }

        private void TimerCallback(string sessionId)
        {
            StopTimer(sessionId);
            OnTimerElapsed?.Invoke(sessionId);
        }

        public void Dispose()
        {
            foreach (var timer in _timers.Values)
            {
                timer?.Dispose();
            }
            _timers.Clear();
            _endTimes.Clear();
        }
    }
}