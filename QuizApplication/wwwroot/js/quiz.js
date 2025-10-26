class QuizTimer {
    constructor(durationInMinutes) {
        this.duration = durationInMinutes * 60; // Convert to seconds
        this.remainingTime = this.duration;
        this.timerInterval = null;
        this.isRunning = false;
    }

    start() {
        if (this.isRunning) return;

        this.isRunning = true;
        this.timerInterval = setInterval(() => {
            this.remainingTime--;
            this.updateDisplay();

            if (this.remainingTime <= 0) {
                this.stop();
                this.onTimeUp();
            }

            // Warning when 2 minutes left
            if (this.remainingTime === 120) {
                this.showWarning();
            }
        }, 1000);
    }

    stop() {
        this.isRunning = false;
        if (this.timerInterval) {
            clearInterval(this.timerInterval);
            this.timerInterval = null;
        }
    }

    updateDisplay() {
        const minutes = Math.floor(this.remainingTime / 60);
        const seconds = this.remainingTime % 60;
        const timeString = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

        const timerElement = document.getElementById('time-remaining');
        if (timerElement) {
            timerElement.textContent = timeString;
        }

        // Add warning class when time is low
        const timerContainer = document.getElementById('timer');
        if (timerContainer) {
            if (this.remainingTime <= 120) {
                timerContainer.classList.add('timer-warning');
            } else {
                timerContainer.classList.remove('timer-warning');
            }
        }
    }

    showWarning() {
        // You can add a visual or audio warning here
        console.log('2 minutes remaining!');
    }

    onTimeUp() {
        alert('Time is up! Submitting your quiz...');
        // Auto-submit the form
        const form = document.querySelector('form');
        if (form) {
            const submitButton = form.querySelector('button[value="Submit"]');
            if (submitButton) {
                submitButton.click();
            }
        }
    }
}

// Initialize timer when page loads
document.addEventListener('DOMContentLoaded', function () {
    // Set timer duration (10 minutes total for the quiz)
    const quizTimer = new QuizTimer(10);
    quizTimer.start();
});