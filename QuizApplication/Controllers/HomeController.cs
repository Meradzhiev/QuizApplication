using Microsoft.AspNetCore.Mvc;
using QuizApplication.Models;
using QuizApplication.Services;
using System.Collections.Generic;
using System.Text.Json;

namespace QuizApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly IQuizService _quizService;
        private readonly ITimerService _timerService;

        public HomeController(IQuizService quizService, ITimerService timerService)
        {
            _quizService = quizService;
            _timerService = timerService;

            // Subscribe to timer events
            _timerService.OnTimerElapsed += OnQuizTimerElapsed;
        }

        public IActionResult Index()
        {
            var quizzes = _quizService.GetAllQuizzes();
            return View(quizzes);
        }

        public IActionResult StartQuiz(int id)
        {
            var quiz = _quizService.GetQuizById(id);
            if (quiz == null)
            {
                return NotFound();
            }

            // Create a copy of the quiz to avoid modifying the original
            var quizCopy = CloneQuiz(quiz);

            // Initialize shuffling
            _quizService.InitializeQuizForSession(quizCopy);

            // Calculate quiz duration
            var quizDuration = _quizService.CalculateQuizDuration(quizCopy);

            // Store quiz in session
            HttpContext.Session.SetString($"Quiz_{id}", JsonSerializer.Serialize(quizCopy));
            HttpContext.Session.SetString($"UserAnswers_{id}", JsonSerializer.Serialize(new Dictionary<int, int>()));
            HttpContext.Session.SetString($"QuizStartTime_{id}", DateTime.Now.ToString());
            HttpContext.Session.SetString($"QuizDuration_{id}", quizDuration.ToString());

            // Start the timer
            _timerService.StartTimer($"quiz_{id}", quizDuration);

            var viewModel = new QuizViewModel
            {
                Quiz = quizCopy,
                CurrentQuestionIndex = 0,
                UserAnswers = new Dictionary<int, int>(),
                TimeRemaining = quizDuration
            };

            return View("Quiz", viewModel);
        }

        [HttpPost]
        public IActionResult SubmitAnswer(QuizViewModel model, string action)
        {
            if (model.Quiz == null || model.Quiz.Id == 0)
            {
                return RedirectToAction("Index");
            }

            // Get quiz from session
            var quizJson = HttpContext.Session.GetString($"Quiz_{model.Quiz.Id}");
            var userAnswersJson = HttpContext.Session.GetString($"UserAnswers_{model.Quiz.Id}");

            if (string.IsNullOrEmpty(quizJson))
            {
                return RedirectToAction("StartQuiz", new { id = model.Quiz.Id });
            }

            var quiz = JsonSerializer.Deserialize<Quiz>(quizJson);
            var userAnswers = string.IsNullOrEmpty(userAnswersJson)
                ? new Dictionary<int, int>()
                : JsonSerializer.Deserialize<Dictionary<int, int>>(userAnswersJson);

            // Store current answer
            if (model.SelectedAnswerId > 0)
            {
                var currentQuestion = quiz.Questions[model.CurrentQuestionIndex];
                userAnswers[currentQuestion.Id] = model.SelectedAnswerId;

                // Save updated answers to session
                HttpContext.Session.SetString($"UserAnswers_{model.Quiz.Id}",
                    JsonSerializer.Serialize(userAnswers));
            }

            // Handle navigation
            if (action == "Previous")
            {
                if (model.CurrentQuestionIndex > 0)
                {
                    model.CurrentQuestionIndex--;
                }
            }
            else if (action == "Next")
            {
                if (model.CurrentQuestionIndex < quiz.Questions.Count - 1)
                {
                    model.CurrentQuestionIndex++;
                }
            }
            else if (action == "Submit")
            {
                return CalculateResults(quiz, userAnswers);
            }

            // Get remaining time
            var remainingTime = _timerService.GetRemainingTime($"quiz_{model.Quiz.Id}");

            // Update model and return to view
            model.Quiz = quiz;
            model.UserAnswers = userAnswers;
            model.TimeRemaining = remainingTime;

            return View("Quiz", model);
        }

        [HttpPost]
        public IActionResult SubmitQuiz(int quizId)
        {
            var quizJson = HttpContext.Session.GetString($"Quiz_{quizId}");
            var userAnswersJson = HttpContext.Session.GetString($"UserAnswers_{quizId}");

            if (string.IsNullOrEmpty(quizJson))
            {
                return RedirectToAction("Index");
            }

            var quiz = JsonSerializer.Deserialize<Quiz>(quizJson);
            var userAnswers = string.IsNullOrEmpty(userAnswersJson)
                ? new Dictionary<int, int>()
                : JsonSerializer.Deserialize<Dictionary<int, int>>(userAnswersJson);

            // Stop the timer
            _timerService.StopTimer($"quiz_{quizId}");

            return CalculateResults(quiz, userAnswers);
        }

        public IActionResult TimeUp(int quizId)
        {
            var quizJson = HttpContext.Session.GetString($"Quiz_{quizId}");
            var userAnswersJson = HttpContext.Session.GetString($"UserAnswers_{quizId}");

            if (string.IsNullOrEmpty(quizJson))
            {
                return RedirectToAction("Index");
            }

            var quiz = JsonSerializer.Deserialize<Quiz>(quizJson);
            var userAnswers = string.IsNullOrEmpty(userAnswersJson)
                ? new Dictionary<int, int>()
                : JsonSerializer.Deserialize<Dictionary<int, int>>(userAnswersJson);

            // Stop the timer
            _timerService.StopTimer($"quiz_{quizId}");

            var result = new QuizResult
            {
                Quiz = quiz,
                UserAnswers = userAnswers,
                Score = _quizService.CalculateScore(quiz, userAnswers),
                WasTimeUp = true
            };

            // Clear session data
            HttpContext.Session.Remove($"Quiz_{quizId}");
            HttpContext.Session.Remove($"UserAnswers_{quizId}");
            HttpContext.Session.Remove($"QuizStartTime_{quizId}");
            HttpContext.Session.Remove($"QuizDuration_{quizId}");

            return View("Results", result);
        }

        private void OnQuizTimerElapsed(string sessionId)
        {
            // Extract quiz ID from session ID
            if (sessionId.StartsWith("quiz_") && int.TryParse(sessionId.Substring(5), out int quizId))
            {
                // Redirect to TimeUp action
                // Note: In a real application, you might want to use SignalR for real-time updates
                // This is a simplified approach
            }
        }

        private IActionResult CalculateResults(Quiz quiz, Dictionary<int, int> userAnswers)
        {
            var score = _quizService.CalculateScore(quiz, userAnswers);

            var result = new QuizResult
            {
                Quiz = quiz,
                UserAnswers = userAnswers,
                Score = score,
                WasTimeUp = false
            };

            // Clear session data
            HttpContext.Session.Remove($"Quiz_{quiz.Id}");
            HttpContext.Session.Remove($"UserAnswers_{quiz.Id}");
            HttpContext.Session.Remove($"QuizStartTime_{quiz.Id}");
            HttpContext.Session.Remove($"QuizDuration_{quiz.Id}");

            // Stop the timer
            _timerService.StopTimer($"quiz_{quiz.Id}");

            return View("Results", result);
        }

        public IActionResult RetakeQuiz(int id)
        {
            // Clear previous session data and stop timer
            HttpContext.Session.Remove($"Quiz_{id}");
            HttpContext.Session.Remove($"UserAnswers_{id}");
            HttpContext.Session.Remove($"QuizStartTime_{id}");
            HttpContext.Session.Remove($"QuizDuration_{id}");
            _timerService.StopTimer($"quiz_{id}");

            return RedirectToAction("StartQuiz", new { id });
        }

        // Helper method to clone quiz
        private Quiz CloneQuiz(Quiz original)
        {
            var json = JsonSerializer.Serialize(original);
            return JsonSerializer.Deserialize<Quiz>(json);
        }
    }
}