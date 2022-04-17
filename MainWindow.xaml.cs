using System;
using System.Media;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Timer
{



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static class NativeMethods
        {
            public struct LastInputInfo
            {
                public UInt32 cbSize;
                public UInt32 dwTime;
            }

            [DllImport("user32.dll")]
            public static extern bool GetLastInputInfo(ref LastInputInfo plii);
        }

        public static TimeSpan GetInputIdleTime()
        {
            var plii = new NativeMethods.LastInputInfo();
            plii.cbSize = (UInt32)Marshal.SizeOf(plii);

            if (NativeMethods.GetLastInputInfo(ref plii))
            {
                return TimeSpan.FromMilliseconds(Environment.TickCount64 - plii.dwTime);
            }
            else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public static DateTimeOffset GetLastInputTime()
        {
            return DateTimeOffset.Now.Subtract(GetInputIdleTime());
        }


        private int WorkMinTemp = 0;
        private int WorkSecTemp = 0;

        private int WorkMin = 0;
        private int WorkSec = 0;

        private int BreakMin = 0;
        private int BreakSec = 0;

        private string WorkMinString = string.Empty;
        private string WorkSecString = string.Empty;

        private string WorkCountDown = string.Empty;

        double progress = 0;

        private DispatcherTimer SecondTimer = new DispatcherTimer();
        private DispatcherTimer workTimer = new DispatcherTimer();
        public DispatcherTimer breakTimer = new DispatcherTimer();

        private MediaPlayer mplayer = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
            button1.Focus();
            paddtextboxes();
            SecondTimer.Tick += new EventHandler(SecondTimer_tick);
            workTimer.Tick += new EventHandler(workcountdowntimer_tick);
            breakTimer.Tick += new EventHandler(BreakTimer_Tick);
            SecondTimer.Interval = new TimeSpan(0, 0, 1);
            SecondTimer.Start();

            startup();
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {

            StartTimers();
        }

        public void startup()
        {
            WorkMinTextBox.Text = "45";
            WorkSecTextBox.Text = "0";
            breakMinTextBox.Text = "15";
            breakSecTextBox.Text = "0";
            StartTimers();

        }

        public void StartTimers()
        {
            WorkMin = Convert.ToInt32(WorkMinTextBox.Text);
            WorkSec = Convert.ToInt32(WorkSecTextBox.Text);
            BreakMin = Convert.ToInt32(breakMinTextBox.Text);
            BreakSec = Convert.ToInt32(breakSecTextBox.Text);

            label2.Content = WorkMin + ":" + WorkSec;
            label5.Content = "Break";

            workTimer.Interval = new TimeSpan(0, WorkMin, WorkSec);
            workTimer.Start();

            breakTimer.Interval = new TimeSpan(0, BreakMin, BreakSec);
        }


        private void BreakTimer_Tick(object sender, EventArgs e)
        {
            workTimer.Start();
            breakTimer.Stop();
            label2.Content = WorkMin + ":" + WorkSec;
            label5.Content = BreakMin + ":" + BreakSec;

            if (GetInputIdleTime().TotalMinutes <= BreakMin * 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    var uri = new Uri(@"breakfinished.wav", UriKind.Relative);
                    mplayer.Open(uri);
                    mplayer.Play();
                    Thread.Sleep(1000);
                }
            }
        }

        private void workcountdowntimer_tick(object sender, EventArgs e)
        {
            workTimer.Stop();
            breakTimer.Start();
            label5.Content = BreakMin + ":" + BreakSec;
            label2.Content = WorkMin + ":" + WorkSec;

            if (GetInputIdleTime().TotalMinutes <= BreakMin * 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    var uri = new Uri(@"workfinished.wav", UriKind.Relative);
                    mplayer.Open(uri);
                    mplayer.Play();
                    Thread.Sleep(1000);
                }
            }
        }


        private void SecondTimer_tick(object sender, EventArgs e)
        {

            if (GetInputIdleTime().TotalMinutes > BreakMin * 2)
            {
                StartTimers();
            }

            else
            {
                if (workTimer.IsEnabled)
                {
                    string currentCountDown = label2.Content.ToString();

                    string[] tempArray = currentCountDown.Split(":");

                    int min = Convert.ToInt32(tempArray[0]);
                    int sec = Convert.ToInt32(tempArray[1]);

                    double secondsRemaining = min * 60 + sec;
                    double totalWorkSeconds = WorkMin * 60 + WorkSec;
                    double secondsCompleted = totalWorkSeconds - secondsRemaining;
                    progress = secondsCompleted / totalWorkSeconds;

                    if (sec > 0)
                    {
                        sec -= 1;
                    }
                    else
                    {
                        sec = 59;
                        min -= 1;
                    }

                    label2.Content = min + ":" + sec;
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                    TaskbarItemInfo.ProgressValue = progress;
                }

                if (breakTimer.IsEnabled)
                {
                    string currentCountDown = label5.Content.ToString();

                    string[] tempArray = currentCountDown.Split(":");

                    int min = Convert.ToInt32(tempArray[0]);
                    int sec = Convert.ToInt32(tempArray[1]);

                    double secondsRemaining = min * 60 + sec;
                    double totalWorkSeconds = BreakMin * 60 + BreakSec;
                    double secondsCompleted = totalWorkSeconds - secondsRemaining;
                    progress = secondsCompleted / totalWorkSeconds;

                    if (sec > 0)
                    {
                        sec -= 1;
                    }
                    else
                    {
                        sec = 59;
                        min -= 1;
                    }

                    label5.Content = min + ":" + sec;
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
                    TaskbarItemInfo.ProgressValue = progress;
                }

            }
            //if (Convert.ToInt32(WorkMinTextBox.Text) != WorkMin)
            //{
            //    WorkMin = Convert.ToInt32(WorkMinTextBox.Text);
            //    WorkSec = Convert.ToInt32(WorkSecTextBox.Text);
            //    WorkMinTemp = WorkMin;
            //    WorkSecTemp = WorkSec;
            //}

            //if (Convert.ToInt32(WorkSecTextBox.Text) != WorkSec)
            //{
            //    WorkMin = Convert.ToInt32(WorkMinTextBox.Text);
            //    WorkSec = Convert.ToInt32(WorkSecTextBox.Text);
            //    WorkMinTemp = WorkMin;
            //    WorkSecTemp = WorkSec;
            //}


            //WorkMinString = WorkMinTemp.ToString();
            //WorkSecString = WorkSecTemp.ToString();

            //WorkCountDown = string.Empty;

            //if (WorkSecTemp > 0)
            //{
            //    WorkSecTemp = WorkSecTemp - 1;

            //    if (WorkSecTemp < 10)
            //        WorkSecString = "0" + WorkSecTemp;
            //    else
            //        WorkSecString = WorkSecTemp.ToString();

            //    if (WorkMinTemp < 10)
            //        WorkMinString = "0" + WorkMinTemp;
            //    else
            //        WorkMinString = WorkMinTemp.ToString();

            //    WorkCountDown = WorkMinString + ":" + WorkSecString;
            //    label2.Content = WorkCountDown;
            //}

            //else //gworksec=0
            //{
            //    if (WorkMinTemp > 0)
            //    {
            //        WorkMinTemp = WorkMinTemp - 1;

            //        if (WorkMinTemp < 10)
            //            WorkMinString = "0" + WorkMinTemp;
            //        else
            //            WorkMinString = WorkMinTemp.ToString();

            //        WorkCountDown = WorkMinString + ":00";
            //        label2.Content = WorkCountDown;

            //        WorkSecTemp = 59;

            //    }
            //    else//gworkmin=0
            //    {
            //        workTimer.Stop();
            //        mplayer.Open(new Uri(@"..\..\sounds\theme1\loud beep.wav", UriKind.RelativeOrAbsolute));
            //        mplayer.Play();

            //    }
            //}
        }

        private void OnKeyUpHandler(object sender, KeyEventArgs e)
        {

        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        #region unused
        // Use the DataObject.Pasting Handler 
        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));

                if (!IsTextAllowed(text)) e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }

        }

        private void WorkMinTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void WorkSecTextBox_PreviewTextInput_1(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void breakMinTextBox_PreviewTextInput_1(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void breakSecTextBox_PreviewTextInput_1(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void textBox6_PreviewTextInput_1(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void textBox6_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            button1.Focus();

        }

        private void paddtextboxes()
        {
            //if (WorkMinTextBox.Text.Length == 0)
            //    WorkMinTextBox.Text = "00";
            //if (WorkSecTextBox.Text.Length == 0)
            //    WorkSecTextBox.Text = "00";
            //if (breakMinTextBox.Text.Length == 0)
            //    breakMinTextBox.Text = "00";
            //if (breakSecTextBox.Text.Length == 0)
            //    breakSecTextBox.Text = "00";
            //if (textBox6.Text.Length == 0)
            //    textBox6.Text = "00";

            //if (WorkMinTextBox.Text.Length == 1)
            //    WorkMinTextBox.Text = "0" + WorkMinTextBox.Text;
            //if (WorkSecTextBox.Text.Length == 1)
            //    WorkSecTextBox.Text = "0" + WorkSecTextBox.Text;
            //if (breakMinTextBox.Text.Length == 1)
            //    breakMinTextBox.Text = "0" + breakMinTextBox.Text;
            //if (breakSecTextBox.Text.Length == 1)
            //    breakSecTextBox.Text = "0" + breakSecTextBox.Text;
            //if (textBox6.Text.Length == 1)
            //    textBox6.Text = "0" + textBox6.Text;
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {

            #region decrease work time

            if ((Keyboard.Modifiers == ModifierKeys.Shift) && (e.Key == Key.W))
            {


                paddtextboxes();

                int workmin = Convert.ToInt32(WorkMinTextBox.Text);
                int worksec = Convert.ToInt32(WorkSecTextBox.Text);

                if (worksec > 0)
                {
                    worksec = worksec - 1;

                    if (worksec.ToString().Length == 1)
                        WorkSecTextBox.Text = "0" + worksec.ToString();
                    else
                        WorkSecTextBox.Text = worksec.ToString();
                }

                else
                {
                    if (workmin > 0)
                    {
                        workmin = workmin - 1;

                        if (workmin.ToString().Length == 1)
                            WorkMinTextBox.Text = "0" + workmin.ToString();
                        else
                            WorkMinTextBox.Text = workmin.ToString();

                        worksec = 59;
                        WorkSecTextBox.Text = worksec.ToString();
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }

            }

            #endregion

            #region increase work time

            else if (Keyboard.IsKeyDown(Key.W))
            {


                paddtextboxes();

                int workmin = Convert.ToInt32(WorkMinTextBox.Text);
                int worksec = Convert.ToInt32(WorkSecTextBox.Text);

                if (worksec >= 59)
                {

                    if (workmin < 99)
                    {
                        worksec = worksec - 59;
                        workmin = workmin + 1;

                        if (workmin.ToString().Length == 1)
                            WorkMinTextBox.Text = "0" + workmin.ToString();
                        else
                            WorkMinTextBox.Text = workmin.ToString();


                        if (worksec.ToString().Length == 1)
                            WorkSecTextBox.Text = "0" + worksec.ToString();
                        else
                            WorkSecTextBox.Text = worksec.ToString();

                    }

                    else
                    {

                        e.Handled = true;
                    }


                }
                else
                {
                    worksec++;
                    if (worksec.ToString().Length == 1)
                        WorkSecTextBox.Text = "0" + worksec.ToString();
                    else
                        WorkSecTextBox.Text = worksec.ToString();
                }


            }

            #endregion

            #region decrease break time


            if ((Keyboard.Modifiers == ModifierKeys.Shift) && (e.Key == Key.B))
            {


                paddtextboxes();

                int breakmin = Convert.ToInt32(breakMinTextBox.Text);
                int breaksec = Convert.ToInt32(breakSecTextBox.Text);

                if (breaksec > 0)
                {
                    breaksec = breaksec - 1;

                    if (breaksec.ToString().Length == 1)
                        breakSecTextBox.Text = "0" + breaksec.ToString();
                    else
                        breakSecTextBox.Text = breaksec.ToString();
                }

                else
                {
                    if (breakmin > 0)
                    {
                        breakmin = breakmin - 1;

                        if (breakmin.ToString().Length == 1)
                            breakMinTextBox.Text = "0" + breakmin.ToString();
                        else
                            breakMinTextBox.Text = breakmin.ToString();

                        breaksec = 59;
                        breakSecTextBox.Text = breaksec.ToString();
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }

            }

            #endregion

            #region increase break time

            else if (Keyboard.IsKeyDown(Key.B))
            {

                if (breakMinTextBox.Text.Length == 0)
                    breakMinTextBox.Text = "00";
                if (breakSecTextBox.Text.Length == 0)
                    breakSecTextBox.Text = "00";

                int breakmin = Convert.ToInt32(breakMinTextBox.Text);
                int breaksec = Convert.ToInt32(breakSecTextBox.Text);

                if (breaksec >= 59)
                {

                    if (breakmin < 99)
                    {
                        breaksec = breaksec - 59;
                        breakmin = breakmin + 1;


                        if (breakmin.ToString().Length == 1)
                            breakMinTextBox.Text = "0" + breakmin.ToString();
                        else
                            breakMinTextBox.Text = breakmin.ToString();



                        if (breaksec.ToString().Length == 1)
                            breakSecTextBox.Text = "0" + breaksec.ToString();
                        else
                            breakSecTextBox.Text = breaksec.ToString();


                    }

                    else
                    {

                        e.Handled = true;
                    }


                }
                else
                {
                    breaksec++;

                    if (breaksec.ToString().Length == 1)
                        breakSecTextBox.Text = "0" + breaksec.ToString();
                    else
                        breakSecTextBox.Text = breaksec.ToString();

                }
            }

            #endregion

            #region decrease repeitions

            if ((Keyboard.Modifiers == ModifierKeys.Shift) && (e.Key == Key.R))
            {

                if (textBox6.Text.Length == 0)
                    textBox6.Text = "00";



                int reps = Convert.ToInt32(textBox6.Text);


                if (reps > 0)
                {


                    reps = reps - 1;
                    if (reps.ToString().Length == 1)
                        textBox6.Text = "0" + reps.ToString();
                    else
                        textBox6.Text = reps.ToString();
                }

                else
                {

                    e.Handled = true;
                }


            }



            #endregion

            #region increase repeitions

            else if (Keyboard.IsKeyDown(Key.R))
            {

                if (textBox6.Text.Length == 0)
                    textBox6.Text = "00";



                int reps = Convert.ToInt32(textBox6.Text);


                if (reps < 99)
                {


                    reps = reps + 1;
                    if (reps.ToString().Length == 1)
                        textBox6.Text = "0" + reps.ToString();
                    else
                        textBox6.Text = reps.ToString();
                }

                else
                {

                    e.Handled = true;
                }


            }



            #endregion

        }

        #endregion

    }
}
