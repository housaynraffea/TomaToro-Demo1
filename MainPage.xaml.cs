using System.Diagnostics;

namespace TomaToro
{
    public partial class MainPage : ContentPage
    {
        int studySessionTime = 0;
        int shortBreakTime = 0;
        int longBreakTime = 0;

        int studyCounter = 0; // how many times we've studied

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

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnStartTimerClicked(object? sender, EventArgs e)
        {
            if (txtStudyTimer.Text != String.Empty && txtShortBreakTimer.Text != String.Empty &&
                txtLongBreakTimer.Text != String.Empty && txtLongBreakInterval.Text != String.Empty)
            {
                SetupSession();

                int secondsLeft = 1;

                if (progress == (int)PROGRESS.Study)
                {
                    secondsLeft = studySessionTime;
                }
                else if (progress == (int)PROGRESS.Break)
                {
                    if (studyCounter == longBreakInterval)
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
                
                string timeLeft = ""; 

                var timer = Dispatcher.CreateTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                
                timer.Tick += (s, e) =>
                {
                    secondsLeft--;
                    
                    MainThread.BeginInvokeOnMainThread(() =>
                    {

                        timeLeft = string.Format("{0}:{1:00}", secondsLeft/60, (Math.Abs(secondsLeft)) % 60);
                        lbTimer.Text = timeLeft;

                        if (studyCounter >= longBreakInterval)
                        {
                            studyCounter = 0;
                        }

                        if (secondsLeft <= 0)
                        {
                            if (progress == 2)
                            {
                                timer.Stop();
                                DisplayAlertAsync("Alert", "Break time over, time to study!", "Okay");
                                progress = 1;
                            }
                            else if (progress == 1)
                            {
                                timer.Stop();
                                DisplayAlertAsync("Alert", "Study time over, time to take a break!", "Okay");
                                progress += 1;
                                studyCounter++;
                            }
                        }
                    });
                };

                timer.Start();
            }
            else
            {
                lbWarning.Text = "Please fill in the blank textboxes!";
                lbWarning.TextColor = Color.FromArgb("FF0000");
            }
        }

        void SetupSession()
        {
            studySessionTime = Convert.ToInt32(txtStudyTimer.Text);
            shortBreakTime = Convert.ToInt32(txtShortBreakTimer.Text);
            longBreakTime = Convert.ToInt32(txtLongBreakTimer.Text);
            longBreakInterval = Convert.ToInt32(txtLongBreakInterval.Text);
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
