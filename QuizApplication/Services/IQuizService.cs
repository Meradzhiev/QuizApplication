using QuizApplication.Models;

namespace QuizApplication.Services
{
    public interface IQuizService
    {
        List<Quiz> GetAllQuizzes();
        Quiz GetQuizById(int id);
        Quiz LoadQuizFromFile(string filePath);
        void ShuffleQuestions(Quiz quiz);
        void ShuffleAnswers(Question question);
        int CalculateScore(Quiz quiz, Dictionary<int, int> userAnswers);
        public void InitializeQuizForSession(Quiz quiz)
        {
            if (quiz == null) return;

            // Shuffle questions
            ShuffleQuestions(quiz);

            // Shuffle answers for each question
            foreach (var question in quiz.Questions)
            {
                ShuffleAnswers(question);
            }
        }

        public TimeSpan CalculateQuizDuration(Quiz quiz)
        {
            // 2 minutes per question
            return TimeSpan.FromMinutes(2 * quiz.Questions.Count);
        }

        public int GetTimePerQuestion(Quiz quiz)
        {
            return 2; // 2 minutes per question
        }
    }
}
