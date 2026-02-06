using Plugin.LocalNotification;
using System.Diagnostics;

namespace TomaToro
{
    public partial class MainPage : ContentPage
    {
        bool setupComplete = false;
        int studySessionTime = 0;
        int shortBreakTime = 0;
        int longBreakTime = 0;

        int totalStudyCounter = 1; // How many times we've studied IN TOTAL (we start with the first session...)
        int currentStudyCounter = 1; // How many times we've studied in one interval (has to be reset if we go over the interval limit)

        int longBreakInterval = 0;

        //bool autoStartSessions = false;
        //bool autoStartBreaks = false;

        enum PROGRESS
        {
            Study = 1,
            Break = 2
        }

        int progress = (int) PROGRESS.Study;

        enum BREAK_TYPE
        {
            ShortBreak = 0,
            LongBreak = 1,
        }

        int breakType = (int) BREAK_TYPE.ShortBreak;

        int secondsLeft = 1;

        IDispatcherTimer timer;
        DateTime backgroundTime;

        public MainPage()
        {
            InitializeComponent();
            SetupSession();
        }

        private void OnTimerBtnClicked(object? sender, EventArgs e)
        {
            if (setupComplete == true)
            {
                if (!timer.IsRunning)
                {
                    timer.Start();
                    TimerBtn.Text = "STOP";

                    ScheduleNotification(secondsLeft);
                }
                else
                {
                    timer.Stop();
                    TimerBtn.Text = "START";

                    LocalNotificationCenter.Current.Cancel(100);
                }
            }
            else
            {
                timer.Start();
            }
        }

        private void OnSkipSessionBtnClicked(object? sender, EventArgs e)
        {
            if (timer.IsRunning)
            {
                timer.Stop();
                TimerBtn.Text = "START";
            }

            if (progress == (int)PROGRESS.Study)
            {
                progress += 1;
            }
            else if (progress == (int)PROGRESS.Break)
            {
                totalStudyCounter++;
                currentStudyCounter++;
                progress = 1;
            }
            
            UpdateSessionProgress();
        }

        // This should be called **ONCE**!
        void SetupSession()
        {
            UpdateSession();

            timer = Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => OnTick();

            UpdateSessionProgress();
            
            setupComplete = true;
        }

        // This can be called **ANYTIME**! Preferably when we've updated the entries when running the app
        void UpdateSession()
        {
            Int32.TryParse(txtStudyTimer.Text, out studySessionTime);
            Int32.TryParse(txtShortBreakTimer.Text, out shortBreakTime);
            Int32.TryParse(txtLongBreakTimer.Text, out longBreakTime);
            Int32.TryParse(txtLongBreakInterval.Text, out longBreakInterval);
        }

        void OnTick()
        {
            if (timer.IsRunning)
                secondsLeft--;

            lbTimer.Text = string.Format("{0:00}:{1:00}", secondsLeft / 60, (Math.Abs(secondsLeft)) % 60);
        }

        void UpdateSessionProgress()
        {
            UpdateSession();

            if (progress == (int)PROGRESS.Study)
            {
                secondsLeft = studySessionTime;
            }
            else if (progress == (int)PROGRESS.Break)
            {
                if (currentStudyCounter % longBreakInterval == 0)
                {
                    breakType = (int)BREAK_TYPE.LongBreak;
                }
                else
                {
                    breakType = (int)BREAK_TYPE.ShortBreak;
                }

                if (breakType == (int)BREAK_TYPE.ShortBreak)
                {
                    secondsLeft = shortBreakTime;
                }
                else
                {
                    secondsLeft = longBreakTime;
                }
            }

            secondsLeft *= 60;

            if (currentStudyCounter >= longBreakInterval)
            {
                currentStudyCounter = 0;
            }

            if (timer.IsRunning)
            {
                TimerBtn.Text = "STOP";
            }
            else
            {
                TimerBtn.Text = "START";
            }

            ShowSessionProgress();
        }

        void ShowSessionProgress()
        {
            lbTimer.Text = string.Format("{0:00}:{1:00}", secondsLeft / 60, (Math.Abs(secondsLeft)) % 60);

            if (progress == (int)PROGRESS.Study)
            {
                lbSessionType.Text = "STUDY";
            }
            else
            {
                if (breakType == (int)BREAK_TYPE.ShortBreak)
                {
                    lbSessionType.Text = "SHORT BREAK";
                }
                else
                {
                    lbSessionType.Text = "LONG BREAK";
                }
            }

            lbSessionCounter.Text = $"Session #{totalStudyCounter}";
        }

        void ScheduleNotification(int secondsLeft)
        {
            string titleText;
            string descriptionText;

            if (progress == (int)PROGRESS.Study)
            {
                titleText = "Study time over!";
                descriptionText = "Time to take a break!";
            }
            else
            {
                titleText = "Break time over!";
                descriptionText = "Time to study!";
            }

            var request = new NotificationRequest
            {
                NotificationId = 100,
                Title = titleText,
                Description = descriptionText,
                BadgeNumber = 42,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now.AddSeconds(secondsLeft)
                }
            };

            LocalNotificationCenter.Current.Show(request);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (timer.IsRunning && backgroundTime != default)
            {
                var backgroundElapsedTime = DateTime.Now - backgroundTime;
                secondsLeft -= (int) backgroundElapsedTime.TotalSeconds;
            }

            if (secondsLeft <= 0)
            {
                timer.Stop();

                if (progress == 2)
                {
                    totalStudyCounter++;
                    currentStudyCounter++;
                    progress = 1;
                }
                else if (progress == 1)
                {
                    progress += 1;
                }

                UpdateSessionProgress();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            backgroundTime = DateTime.Now;
        }

        /*
        void OnAutoStartSessionsToggled(object? sender, EventArgs e)
        {
            autoStartSessions = swAutoStartSessions.IsToggled;
        }
        void OnAutoStartBreaksToggled(object? sender, EventArgs e)
        {
            autoStartBreaks = swAutoStartBreaks.IsToggled;
        }
        */
    }
}
