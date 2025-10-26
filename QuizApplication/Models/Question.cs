using System.Text.Json.Serialization;

namespace QuizApplication.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<Answer> Answers { get; set; } = new List<Answer>();
        public int CorrectAnswerId { get; set; }

        [JsonIgnore]
        public bool IsShuffled { get; set; }
    }
}
