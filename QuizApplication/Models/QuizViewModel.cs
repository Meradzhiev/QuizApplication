namespace QuizApplication.Models
{
    public class QuizViewModel
    {
        public Quiz Quiz { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public int SelectedAnswerId { get; set; }
        public Dictionary<int, int> UserAnswers { get; set; } = new Dictionary<int, int>();
        public TimeSpan TimeRemaining { get; set; }
    }
}
