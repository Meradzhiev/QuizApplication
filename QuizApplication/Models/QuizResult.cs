namespace QuizApplication.Models
{
    public class QuizResult
    {
        public Quiz Quiz { get; set; }
        public Dictionary<int, int> UserAnswers { get; set; } = new Dictionary<int, int>();
        public int Score { get; set; }
        public int TotalQuestions => Quiz?.Questions.Count ?? 0;
        public double Percentage => TotalQuestions > 0 ? (double)Score / TotalQuestions * 100 : 0;
        public bool WasTimeUp { get; set; }
    }
}
