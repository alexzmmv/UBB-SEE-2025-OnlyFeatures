using System.Collections.ObjectModel;
using System.Windows.Input;
using CourseApp.Models;
using CourseApp.Services;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using System;
using Microsoft.UI.Xaml.Controls;


namespace CourseApp.ViewModels

{
    public class CourseViewModel : BaseViewModel
    {
        private DispatcherTimer? timer;
        private DateTime sessionStartTime;
        private int totalTimeSpent;
        private readonly CourseService courseService;
        private readonly CoinsService coinsService;
        public Course CurrentCourse { get; set; }
        public ObservableCollection<Models.Module> Modules { get; set; }
        public ICommand EnrollCommand { get; set; }
        public bool IsEnrolled { get; set; }
        public int CompletedModules { get; private set; }
        public int RequiredModules { get; private set; }
        public bool IsCourseCompleted => CompletedModules >= RequiredModules;
        public int TimeLimit { get; private set; }
        public int TimeRemaining => Math.Max(0, TimeLimit - totalTimeSpent);
        public int TimedRewardAmount { get; private set; }
        public bool CompletionRewardClaimed { get; private set; } = false;
        public bool TimedRewardClaimed { get; private set; } = false;
        public int CoinBalance
        {
            get => coinsService.GetUserCoins(0);
        }

        private string timeSpent;
        private bool timerStarted;
        private int lastSavedTime = 0;

        // Add notification properties
        private string notificationMessage = string.Empty;
        private bool showNotification = false;

        public string NotificationMessage
        {
            get => notificationMessage;
            set
            {
                notificationMessage = value;
                OnPropertyChanged(nameof(NotificationMessage));
            }
        }

        public bool ShowNotification
        {
            get => showNotification;
            set
            {
                showNotification = value;
                OnPropertyChanged(nameof(ShowNotification));
            }
        }

        /// <summary>
        /// Formatted string showing the current tracked time spent on the course.
        /// Updated every second while the timer is running.
        /// </summary>
        public string TimeSpent
        {
            get => timeSpent;
            set
            {
                timeSpent = value;
                OnPropertyChanged(nameof(TimeSpent));
            }
        }
        public CourseViewModel(Course course)
        {
            courseService = new CourseService();
            coinsService = new CoinsService();
            coinsService.GetUserCoins(0);
            CurrentCourse = course;
            Modules = new ObservableCollection<Models.Module>(courseService.GetModules(course.CourseId));
            IsEnrolled = courseService.IsUserEnrolled(course.CourseId);
            EnrollCommand = new RelayCommand(ExecuteEnroll, CanEnroll);

            CompletedModules = courseService.GetCompletedModulesCount(course.CourseId);
            RequiredModules = courseService.GetRequiredModulesCount(course.CourseId);
            OnPropertyChanged(nameof(CompletedModules));
            OnPropertyChanged(nameof(RequiredModules));
            OnPropertyChanged(nameof(IsCourseCompleted));

            TimeLimit = courseService.GetCourseTimeLimit(course.CourseId);
            TimedRewardAmount = courseService.GetTimedRewardAmount(course.CourseId);

            totalTimeSpent = courseService.GetTimeSpent(course.CourseId);
            lastSavedTime = totalTimeSpent;

            TimeSpent = FormatCountdownTime(TimeRemaining);

            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) =>
            {
                totalTimeSpent++;
                TimeSpent = FormatCountdownTime(TimeRemaining);
                OnPropertyChanged(nameof(TimeRemaining));
            };
        }

        public void UpdateModuleCompletion(int moduleId)
        {
            // Mark module as completed
            courseService.CompleteModule(moduleId, CurrentCourse.CourseId);

            // Update completion counts
            CompletedModules = courseService.GetCompletedModulesCount(CurrentCourse.CourseId);
            OnPropertyChanged(nameof(CompletedModules));
            OnPropertyChanged(nameof(IsCourseCompleted));

            // Check if course is now completed
            if (IsCourseCompleted)
            {
                // Try to claim the completion reward
                bool rewardClaimed = courseService.ClaimCompletionReward(CurrentCourse.CourseId);
                if (rewardClaimed)
                {
                    CompletionRewardClaimed = true;
                    OnPropertyChanged(nameof(CompletionRewardClaimed));
                    OnPropertyChanged(nameof(CoinBalance));

                    // Show reward notification
                    NotifyCompletionReward();
                }

                // Try to claim the timed reward
                if (TimeRemaining > 0)
                {
                    bool timedRewardClaimed = courseService.ClaimTimedReward(CurrentCourse.CourseId, totalTimeSpent);
                    if (timedRewardClaimed)
                    {
                        TimedRewardClaimed = true;
                        OnPropertyChanged(nameof(TimedRewardClaimed));
                        OnPropertyChanged(nameof(CoinBalance));

                        // Show timed reward notification
                        NotifyTimedReward();
                    }
                }
            }
        }

        // Replace dialog notifications with TextBlock notifications
        private void NotifyCompletionReward()
        {
            NotificationMessage = "Congratulations! You have completed all required modules in this course. 50 coins have been added to your balance.";
            ShowNotification = true;

            // Make the text disappear after a few seconds
            DispatcherTimer notificationTimer = new DispatcherTimer();
            notificationTimer.Interval = TimeSpan.FromSeconds(5);
            notificationTimer.Tick += (s, e) => {
                ShowNotification = false;
                notificationTimer.Stop();
            };
            notificationTimer.Start();
        }

        private void NotifyTimedReward()
        {
            NotificationMessage = $"Congratulations! You completed the course within the time limit. {TimedRewardAmount} coins have been added to your balance.";
            ShowNotification = true;

            // Make the text disappear after a few seconds
            DispatcherTimer notificationTimer = new DispatcherTimer();
            notificationTimer.Interval = TimeSpan.FromSeconds(5);
            notificationTimer.Tick += (s, e) => {
                ShowNotification = false;
                notificationTimer.Stop();
            };
            notificationTimer.Start();
        }

        // Replace existing FormatTime with FormatCountdownTime
        private string FormatCountdownTime(int seconds)
        {
            if (seconds <= 0)
            {
                return "Time's up!";
            }

            var ts = TimeSpan.FromSeconds(seconds);
            return $"{ts.Minutes + ts.Hours * 60} min {ts.Seconds} sec";
        }

        private bool CanEnroll(object parameter)
        {
            return !IsEnrolled;
        }

        /// <summary>
        /// Handles course enrollment logic. Sets IsEnrolled to true, resets the timer to 0,
        /// and starts timing from scratch for a new enrollment.
        /// </summary>
        private void ExecuteEnroll(object parameter)
        {
            courseService.EnrollInCourse(CurrentCourse.CourseId);
            IsEnrolled = true;
            totalTimeSpent = 0;
            TimeSpent = FormatTime(totalTimeSpent);
            OnPropertyChanged(nameof(IsEnrolled));
            StartTimer();
        }

        /// <summary>
        /// Starts the timer for tracking time spent on the course.
        /// This will only run if the course is enrolled and the timer is not already running.
        /// </summary>
        public void StartTimer()
        {
            if (!timerStarted && IsEnrolled)
            {
                timerStarted = true;
                sessionStartTime = DateTime.Now;
                timer?.Start();
            }
        }

        /// <summary>
        /// Stops the timer and saves the newly accumulated time to the database.
        /// Called when the user navigates away from the course or module.
        /// </summary>
        public void PauseTimer()
        {
            if (timerStarted)
            {
                timer?.Stop();
                SaveTimeSpent();
                timerStarted = false;
            }
        }

        /// <summary>
        /// Saves only the new time spent since the last save to the database.
        /// This prevents double-counting time when the app restarts or navigates back.
        /// </summary>
        private void SaveTimeSpent()
        {
            int delta = totalTimeSpent - lastSavedTime;

            if (delta > 0)
            {
                courseService.UpdateTimeSpent(CurrentCourse.CourseId, delta);
                lastSavedTime = totalTimeSpent;
            }
        }

        /// <summary>
        /// Converts a time value in seconds into a formatted string like "X min Y sec".
        /// </summary>
        /// <param name="seconds">The number of seconds to format.</param>
        /// <returns>Formatted string representing the time.</returns>
        private string FormatTime(int seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            return $"{ts.Minutes + ts.Hours * 60} min {ts.Seconds} sec";
        }
    }
}