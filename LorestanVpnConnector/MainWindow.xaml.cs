using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using MessageBox = System.Windows.Forms.MessageBox;


namespace LorestanVpnConnector
{

    public partial class MainWindow
    {

        private readonly Connection _connection=new Connection();

        private readonly Thread _thread;

        private bool _stopThread = true;

        enum ButtonState
        {
            None = 0,
            Start,
            Stop
        }
        public Dictionary<string, string> Accounts = new Dictionary<string, string>();



        private ButtonState _bt = ButtonState.None;
        private int _index;

        private ProgressDialogController _pdc;

        private readonly System.Windows.Forms.NotifyIcon _ni;
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                _ni.Visible = true;
            }

            base.OnStateChanged(e);
        }
        public MainWindow()
        {

            _ni =
                new System.Windows.Forms.NotifyIcon
                {
                    Icon = Properties.Resources.icons8_VPN_64_png,
                    Visible = true,
                    Text = @"Lorestan Vpn Connector"
                };
            _ni.Click +=
                delegate
                {
                    Show();
                    _ni.Visible = false;
                    WindowState = WindowState.Normal;
                };

            _thread = new Thread(() =>
            {
                while (true)
                {

                    while (_stopThread)
                    {
                    }

                    if (_index >= Accounts.Count)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                        {
                            StartButton.IsEnabled = true;
                            Status.Title = "Finish";
                            StatusIcon.Visual = (Visual)TryFindResource("appbar_check");
                            _bt = ButtonState.None;
                            StartButton.Content = "Start";
                            Slider.IsEnabled = true;
                            _stopThread = true;
                        });

                        continue;

                    }
                    
                    #region CheckWifi

                    if (!_connection.IsWifiConnected())
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart) async delegate
                        {
                            if (_pdc == null || !_pdc.IsOpen) 
                            {
                                _pdc = await this.ShowProgressAsync("Network Problem",
                                    "Make Sure You're Connected to Lorestan University Network !", true);
                                _pdc.Canceled += (o, args) =>
                                {
                                    DisconnectedMode();
                                    _pdc.CloseAsync();
                                };
                                _pdc.SetIndeterminate();
                            }
                            
                        });


                        while (!_connection.IsWifiConnected())
                            Thread.Sleep(1000);
                        if (_pdc != null && _pdc.IsOpen) 
                            _pdc.CloseAsync();
                    }



                    #endregion

                    while (_stopThread)
                    {
                    }

                    #region CheckInternet

                    try
                    {

                        if (!_connection.IsConnected())
                        {
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart) CheckingMode);


                            var user = Accounts.Keys.ElementAt(_index);
                            var password = Accounts[user];
                            if (_connection.Login(user, password))
                            {
                                //connect shod
                                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                                {
                                    CurrentUsername.Content = user;
                                    CurrentPassword.Content = password;
                                    ConnectedMode();
                                });
                            }
                            else 
                                _index++;
                            
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart) ConnectedMode);
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    #endregion


                    Thread.Sleep(2000);
                }
            }) {IsBackground = false};


            _thread.Start();


            

            InitializeComponent();

            var themes = Skin.GetAllThemes();
            Themes.ItemsSource = themes;
            Themes.SelectedIndex = themes.FindIndex(c => c == Skin.GetTheme());

            var accents = Skin.GetAllAccents();
            Accents.ItemsSource = accents;
            Accents.SelectedIndex = accents.FindIndex(c => c == Skin.GetAccent());
            
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
                    catch(Exception)
                    {
                        this.ShowMessageAsync("Error", "Something bad is happened :(");
                    }
                }
            }
        }

        private void InfoButton_OnClick(object sender, RoutedEventArgs e) => AboutFlyout.IsOpen = true;

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            
            
            if (!_connection.IsWifiConnected())
            {
                await this.ShowMessageAsync("Network Problem", "Make Sure You're Connected to Lorestan University Network !");
                return;
            }

            if (Accounts.Count == 0)
            {
                await this.ShowMessageAsync("Error", "Account List is Empty !");
                return;
            }

            GroupBox1.IsEnabled = false;

            if (_bt == ButtonState.Stop || _bt == ButtonState.None)
                CheckingMode();
            else if (_bt == ButtonState.Start)
            {
                DisconnectedMode();
                try
                {
                    _connection.Logout();
                }
                catch
                {
                }
                
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _thread.Abort();
            }
            catch { }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => _index = Convert.ToInt32(e.NewValue);


        void CheckingMode()
        {
            _bt = ButtonState.Start;
            _stopThread = false;
            StartButton.Content = "Stop";
            Status.Title = "Checking";
            StatusIcon.Visual = (Visual)TryFindResource("appbar_progress");
            Slider.IsEnabled = false;
            CurrentUsername.Content = "--";
            CurrentPassword.Content = "--";
            Slider.Value = _index;
        }

        void ConnectedMode()
        {
            Status.Title = "Connected";
            StatusIcon.Visual = (Visual)TryFindResource("appbar_connection_quality_veryhigh");
            Slider.Value = _index;
            StartButton.Content = "Stop";
            Slider.IsEnabled = false;
            _bt = ButtonState.Start;
        }

        void DisconnectedMode()
        {
            _bt = ButtonState.Stop;
            _stopThread = true;
            StartButton.Content = "Start";
            Slider.Value = _index;
            Slider.IsEnabled = true;
            Status.Title = "Disconnected";
            StatusIcon.Visual = (Visual)TryFindResource("appbar_connection_quality_extremelylow");

        }

        private void ThemeButton_OnClick(object sender, RoutedEventArgs e) => SkinFlyout.IsOpen = true;

        private void SaveChangesButton_OnClick(object sender, RoutedEventArgs e)
        {
            Skin.SetAccentColor(Accents.SelectedValue.ToString());
            Skin.SetTheme(Themes.SelectedValue.ToString());
            SkinFlyout.IsOpen = false;
        }
    }
}
