namespace QuizApplication.Services
{
    public interface ITimerService
    {
        void StartTimer(string sessionId, TimeSpan duration);
        void StopTimer(string sessionId);
        TimeSpan GetRemainingTime(string sessionId);
        bool IsTimerRunning(string sessionId);
        event Action<string> OnTimerElapsed;
    }
}
