using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;


namespace LorestanVpnConnector
{

    public partial class MainWindow
    {

        private Connection _connection=new Connection();

        private Thread thread;

        private bool StopThread = true;

        enum ButtonState
        {
            None = 0,
            Start,
            Stop
        }
        public Dictionary<string, string> Accounts = new Dictionary<string, string>();



        private ButtonState bt = ButtonState.None;
        private int Index = 0;

        private System.Windows.Forms.NotifyIcon ni;
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                ni.Visible = true;
            }

            base.OnStateChanged(e);
        }
        public MainWindow()
        {


            ni =
                new System.Windows.Forms.NotifyIcon
                {
                    Icon = Properties.Resources.icons8_VPN_64_png,
                    Visible = true,
                    Text = "Lorestan Vpn Connector"
                };
            ni.Click +=
                delegate
                {
                    Show();
                    ni.Visible = false;
                    WindowState = WindowState.Normal;
                };

            thread = new Thread(() =>
            {
                while (true)
                {
                    while (StopThread)
                    {
                    }

                    if (Index >= Accounts.Count)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart) delegate
                        {
                            StartButton.IsEnabled = true;
                            Status.Title = "Finish";
                            StatusIcon.Visual = (Visual) TryFindResource("appbar_check");
                            bt=ButtonState.None;
                            StartButton.Content = "Start";
                            Slider.IsEnabled = true;
                            StopThread = true;
                        });
                        
                        continue;

                    }

                    #region CheckInternet

                    try
                    {

                        if (!_connection.IsConnected())
                        {
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart) delegate
                            {
                                Status.Title = "Checking";
                                StatusIcon.Visual = (Visual) TryFindResource("appbar_progress");
                                CurrentUsername.Content = "--";
                                CurrentPassword.Content = "--";
                                Slider.Value = Index;
                            });


                            var user = Accounts.Keys.ElementAt(Index);
                            var password = Accounts[user];
                            if (_connection.Login(user, password))
                            {
                                //connect shod
                                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                                {
                                    Status.Title = "Connected";
                                    StatusIcon.Visual = (Visual)TryFindResource("appbar_connection_quality_veryhigh");
                                    CurrentUsername.Content = user;
                                    CurrentPassword.Content = password;
                                    Slider.Value = Index;
                                });
                            }
                            else 
                                Index++;
                            
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart) delegate
                            {
                                Status.Title = "Connected";
                                StatusIcon.Visual = (Visual) TryFindResource("appbar_connection_quality_veryhigh");
                                Slider.Value = Index;
                            });
                        }
                    }
                    catch
                    {
                    }

                    #endregion


                    Thread.Sleep(2000);
                }
            }) {IsBackground = false};


            thread.Start();

            InitializeComponent();


        }

        void Connect(string username, string password)
        {

            var resp = new MyWebRequest("http://internet.lu.ac.ir/login", "POST",
                $"username={username}&password={password}").GetResponse();
            if (!resp.Contains("login"))
            {
                //connect shod
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                {
                    Status.Title = "Connected";
                    StatusIcon.Visual = (Visual)TryFindResource("appbar_connection_quality_veryhigh");
                    CurrentUsername.Content = username;
                    CurrentPassword.Content = password;
                    Slider.Value = Index;
                });

            }


        }
        
        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Text file(*.txt)|*.txt"
            };
            if (dialog.ShowDialog(this) == true)
            {
                if (dialog.FileName != "")
                {
                    try
                    {
                        TextReader tr = new StreamReader(dialog.FileName);
                        var read = tr.ReadToEnd().Replace("\r","");
                        tr.Close();

                        if (!string.IsNullOrEmpty(read))
                        {
                            Accounts.Clear();

                            foreach (var accs in read.Split('\n'))
                            {
                                if (string.IsNullOrEmpty(accs) || accs.Trim() == "")
                                    continue;
                                var username = accs.Split(':')[0].Trim().Replace("\r", "");
                                var pass = accs.Split(':')[1].Trim().Replace("\r", "");
                                Accounts.Add(username, pass);
                            }
                            FileAddress.Content = dialog.FileName;
                            AccountCount.Content = Accounts.Count;

                            Slider.Maximum = Accounts.Count - 1;

                            GroupBox2.IsEnabled = true;

                        }

                    }
                    catch(Exception exception)
                    {
                        this.ShowMessageAsync("Error", "Something bad is happened :(");
                    }
                }
            }
        }

        private async void InfoButton_OnClick(object sender, RoutedEventArgs e)
        {
            await this.ShowMessageAsync("About Us", "Lorestan Vpn Connector V1.1\n" +
                                                    "\nCoded By :\nAhmadreza Salehvand\n\n" +
                                                    "Testers : \n" +
                                                    "Reza Akrami\nHamid Bayati\nSajad Esmaeili\nAfshin Zafari\n" +
                                                    "\nAnd Special Thanks to :\n" +
                                                    "Mehran Arjang(Shirani)\n" +
                                                    "Ali Dehghani\n" +
                                                    "\nHave Fun :)");
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_connection.IsWifiConnected())
            {
                this.ShowMessageAsync("Network Problem", "Make Sure You're Connected to Lorestan Network !");
                return;
            }

            if (Accounts.Count == 0)
            {
                this.ShowMessageAsync("Error", "Account List is Empty !");
                return;
            }

            GroupBox1.IsEnabled = false;

            if (bt == ButtonState.Stop || bt == ButtonState.None)
            {
                bt = ButtonState.Start;
                StopThread = false;

                StartButton.Content = "Stop";

                Status.Title = "Checking";
                StatusIcon.Visual = (Visual)TryFindResource("appbar_progress");

                Slider.IsEnabled = false;

            }
            else if (bt == ButtonState.Start)
            {
                bt = ButtonState.Stop;
                StopThread = true;
                StartButton.Content = "Start";

                Slider.Value = Index;

                Status.Title = "Disconnected";
                StatusIcon.Visual = (Visual)TryFindResource("appbar_connection_quality_extremelylow");
                try
                {
                    _connection.Logout();
                }
                catch { }

                Slider.IsEnabled = true;
            }






        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                thread.Abort();
            }
            catch { }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => Index = Convert.ToInt32(e.NewValue);
    }
}
