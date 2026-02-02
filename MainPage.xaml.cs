using System.Diagnostics;

namespace TomaToro
{
    public partial class MainPage : ContentPage
    {
        bool setupComplete = false;
        int studySessionTime = 0;
        int shortBreakTime = 0;
        int longBreakTime = 0;

        int totalStudyCounter = 1; // how many times we've studied IN TOTAL (we start with the first session...)
        int currentStudyCounter = 1; // how many times we've studied in one interval

        int longBreakInterval = 0; // used with currentStudyCounter; to find out when we need to switch to long breaks

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
                }
                else
                {
                    timer.Stop();
                    TimerBtn.Text = "START";
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

        void SetupSession()
        {
            Int32.TryParse(txtStudyTimer.Text, out studySessionTime);
            Int32.TryParse(txtShortBreakTimer.Text, out shortBreakTime);
            Int32.TryParse(txtLongBreakTimer.Text, out longBreakTime);
            Int32.TryParse(txtLongBreakInterval.Text, out longBreakInterval);
            
            timer = Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => OnTick();

            UpdateSessionProgress();
            
            setupComplete = true;
        }

        void OnTick()
        {
            if (timer.IsRunning)
            {
                secondsLeft--;
                TimerBtn.Text = "STOP";
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                lbTimer.Text = string.Format("{0:00}:{1:00}", secondsLeft / 60, (Math.Abs(secondsLeft)) % 60);

                if (secondsLeft <= 0)
                {
                    if (progress == 2)
                    {
                        timer.Stop();
                        DisplayAlertAsync("Alert", "Break time over, time to study!", "Okay");
                        totalStudyCounter++;
                        currentStudyCounter++;
                        progress = 1;
                        UpdateSessionProgress();
                    }
                    else if (progress == 1)
                    {
                        timer.Stop();
                        DisplayAlertAsync("Alert", "Study time over, time to take a break!", "Okay");
                        progress += 1;
                        UpdateSessionProgress();
                    }
                }
            });
        }

        void UpdateSessionProgress()
        {
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
