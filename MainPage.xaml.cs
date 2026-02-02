using Android.App;
using System.Diagnostics;

namespace TomaToro
{
    public partial class MainPage : ContentPage
    {
        int studySessionTime = 0;
        int shortBreakTime = 0;
        int longBreakTime = 0;

        int totalStudyCounter = 1; // how many times we've studied IN TOTAL (we start with the first session...)
        int currentStudyCounter = 1; // how many times we've studied in one interval

        int longBreakInterval = 0; // use with studyCounter to find out when we need to switch to long break

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
            ShowSessionProgress();
        }

        private void OnStartTimerClicked(object? sender, EventArgs e)
        {
            if (txtStudyTimer.Text != String.Empty && txtShortBreakTimer.Text != String.Empty &&
                txtLongBreakTimer.Text != String.Empty && txtLongBreakInterval.Text != String.Empty)
            {
                SetupSession();
            }
            else
            {
                lbWarning.Text = "Please fill in the blank textboxes!";
                lbWarning.TextColor = Color.FromArgb("FF0000");
            }

            UpdateSessionProgress();

            if (currentStudyCounter >= longBreakInterval)
            {
                currentStudyCounter = 0;
            }

            timer.Start();
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
        }

        void OnTick()
        {
            secondsLeft--;

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
                        ShowSessionProgress();
                    }
                    else if (progress == 1)
                    {
                        timer.Stop();
                        DisplayAlertAsync("Alert", "Study time over, time to take a break!", "Okay");
                        progress += 1;
                        ShowSessionProgress();
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

            ShowSessionProgress();
        }

        void ShowSessionProgress()
        {
            if (progress == (int)PROGRESS.Study)
            {
                lbSessionType.Text = "STUDY";
            }
            else if (progress == (int)PROGRESS.Break)
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
