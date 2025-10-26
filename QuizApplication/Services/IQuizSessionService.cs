using QuizApplication.Models;

namespace QuizApplication.Services
{
    public interface IQuizSessionService
    {
        string CreateQuizSession(int quizId);
        QuizViewModel GetCurrentQuestion(string sessionId);
        void SaveAnswer(string sessionId, int questionId, int answerId);
        QuizResult GetResults(string sessionId);
        bool SessionExists(string sessionId);
    }
}
