using System.Diagnostics;

namespace TomaToro
{
    public partial class MainPage : ContentPage
    {
        #region initial variables
        bool setupComplete = false;
        int studySessionTime = 0;
        int shortBreakTime = 0;
        int longBreakTime = 0;

        int totalStudyCounter = 1; // How many times we've studied IN TOTAL (we start with the first session...)
        int currentStudyCounter = 1; // How many times we've studied in one interval (has to be reset if we go over the interval limit)

        int longBreakInterval = 0;

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
        #endregion

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

            if (secondsLeft <= 0)
                EndSession();
        }

        void UpdateSessionProgress()
        {
            UpdateSession();

            #region enumerator checks
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
            #endregion

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

        void EndSession()
        {
            timer.Stop();
            secondsLeft = 0;

            if (progress == (int)PROGRESS.Break)
            {
                DisplayAlertAsync("Alert", "Break time is over, time to study!", "Okay");

                totalStudyCounter++;
                currentStudyCounter++;
                progress = 1;
            }
            else
            {
                DisplayAlertAsync("Alert", "Study time is over, time to take a break!", "Okay");
                progress += 1;
            }

            UpdateSessionProgress();
        }
    }
}
