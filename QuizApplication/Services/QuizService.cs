using QuizApplication.Models;
using System.Text.Json;

namespace QuizApplication.Services
{
    public class QuizService : IQuizService
    {
        private readonly List<Quiz> _quizzes;
        private readonly Random _random;

        public QuizService()
        {
            _random = new Random();
            _quizzes = new List<Quiz>();
            InitializeSampleQuizzes();
        }

        private void InitializeSampleQuizzes()
        {
            // Sample C# Quiz
            var csharpQuiz = new Quiz
            {
                Id = 1,
                Title = "C# Programming Quiz",
                Description = "Test your C# knowledge with this beginner quiz"
            };

            csharpQuiz.Questions.AddRange(new[]
            {
                new Question
                {
                    Id = 1,
                    Text = "Which keyword is used to define a class in C#?",
                    Answers = new List<Answer>
                    {
                        new Answer { Id = 1, Text = "class", Letter = 'A' },
                        new Answer { Id = 2, Text = "struct", Letter = 'B' },
                        new Answer { Id = 3, Text = "interface", Letter = 'C' },
                        new Answer { Id = 4, Text = "object", Letter = 'D' }
                    },
                    CorrectAnswerId = 1
                },
                new Question
                {
                    Id = 2,
                    Text = "What is the base class for all classes in .NET?",
                    Answers = new List<Answer>
                    {
                        new Answer { Id = 5, Text = "Object", Letter = 'A' },
                        new Answer { Id = 6, Text = "Base", Letter = 'B' },
                        new Answer { Id = 7, Text = "System", Letter = 'C' },
                        new Answer { Id = 8, Text = "Root", Letter = 'D' }
                    },
                    CorrectAnswerId = 5
                },
                new Question
                {
                    Id = 3,
                    Text = "Which annotation is used to define a controller in ASP.NET MVC?",
                    Answers = new List<Answer>
                    {
                        new Answer { Id = 9, Text = "[Controller]", Letter = 'A' },
                        new Answer { Id = 10, Text = "[MvcController]", Letter = 'B' },
                        new Answer { Id = 11, Text = "Controller class (inheritance)", Letter = 'C' },
                        new Answer { Id = 12, Text = "[WebController]", Letter = 'D' }
                    },
                    CorrectAnswerId = 11
                }
            });

            _quizzes.Add(csharpQuiz);

            // Load additional quizzes from JSON files if they exist
            LoadQuizzesFromFiles();
        }

        public List<Quiz> GetAllQuizzes()
        {
            return _quizzes;
        }

        public Quiz GetQuizById(int id)
        {
            return _quizzes.FirstOrDefault(q => q.Id == id);
        }

        public Quiz LoadQuizFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var quiz = JsonSerializer.Deserialize<Quiz>(json);
                    if (quiz != null)
                    {
                        _quizzes.Add(quiz);
                        return quiz;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading quiz from file: {ex.Message}");
            }
            return null;
        }

        private void LoadQuizzesFromFiles()
        {
            var dataFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            if (Directory.Exists(dataFolder))
            {
                var jsonFiles = Directory.GetFiles(dataFolder, "*.json");
                foreach (var file in jsonFiles)
                {
                    LoadQuizFromFile(file);
                }
            }
        }

        public void ShuffleQuestions(Quiz quiz)
        {
            if (quiz?.Questions == null) return;

            var shuffled = quiz.Questions.OrderBy(x => _random.Next()).ToList();
            quiz.Questions.Clear();
            quiz.Questions.AddRange(shuffled);
        }

        public void ShuffleAnswers(Question question)
        {
            if (question?.Answers == null || question.IsShuffled) return;

            var shuffled = question.Answers.OrderBy(x => _random.Next()).ToList();

            // Update letters after shuffling
            char letter = 'A';
            foreach (var answer in shuffled)
            {
                answer.Letter = letter++;
            }

            question.Answers.Clear();
            question.Answers.AddRange(shuffled);
            question.IsShuffled = true;
        }

        public int CalculateScore(Quiz quiz, Dictionary<int, int> userAnswers)
        {
            if (quiz?.Questions == null) return 0;

            return quiz.Questions.Count(q =>
                userAnswers.ContainsKey(q.Id) && userAnswers[q.Id] == q.CorrectAnswerId);
        }

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
    }
}
