using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using FluentFTP;
using MaterialDesignThemes.Wpf;
using System.Net;
using System.IO;

namespace ModeManager
{
    /// <summary>
    /// Interaction logic for Page2.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        FtpClient client = new FtpClient();
        Grid bgGrid = new Grid();
        Task connectionTask = null;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            client.Host = hostname.Text;
            client.Credentials = new NetworkCredential(username.Text, password.Password);
            client.DataConnectionConnectTimeout = 10000;
            
            connectionTask = client.ConnectAsync();

            Grid bgGrid = new Grid();
            bgGrid.Background = new SolidColorBrush(Colors.Black);
            bgGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            bgGrid.VerticalAlignment = VerticalAlignment.Stretch;
            bgGrid.Opacity = 0.8;

            ProgressBar pb = new ProgressBar();
            pb.Style = (Style)FindResource("MaterialDesignCircularProgressBar");
            pb.Height = 50;
            pb.Width = 50;
            pb.Value = 0;
            pb.IsIndeterminate = true;
            pb.HorizontalAlignment = HorizontalAlignment.Center;
            pb.VerticalAlignment = VerticalAlignment.Center;
            bgGrid.Children.Add(pb);

            MainGrid.Children.Add(bgGrid);

            connectionTask.GetAwaiter().OnCompleted(() =>//#12C06A
            {
                if (connectionTask.Exception == null)
                {
                    CreateLoginFile();
                    (Application.Current.MainWindow as MainWindow).FrmViewer.Navigate(new Uri("Pages/ConfirmationPage.xaml", UriKind.Relative));
                }
                else
                {
                    Snackbar sb = new Snackbar();
                    MainGrid.Children.Add(sb);

                    SnackbarMessageQueue mq = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(3000));
                    sb.MessageQueue = mq;

                    sb.VerticalAlignment = VerticalAlignment.Bottom;
                    //sb.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    sb.Margin = new Thickness(0,0,0,20);


                    if (connectionTask.Exception.Message.Contains("USER"))
                    {
                        sb.MessageQueue.Enqueue("       .שם משתמש או סיסמה לא נכונים");
                    }
                    else if (connectionTask.Exception.Message.Contains("host"))
                    {
                        sb.MessageQueue.Enqueue("      .לא ניתן להתחבר לכתובת המארח");
                    }
                    else
                    {
                        sb.MessageQueue.Enqueue(".שגיאה לא ידועה, לא ניתן ליצור חיבור אל המארח");
                    }
                }

                MainGrid.Children.Remove(bgGrid);
            });
        }

        private void CreateLoginFile()
        {
            string path = Directory.GetCurrentDirectory() + "\\LoginDetails.ini";

            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("[Mode Manager]");
                sw.WriteLine("Host=" + hostname.Text);
                sw.WriteLine("Username=" + username.Text);
                sw.WriteLine("Password=" + password.Password);
                sw.WriteLine("RemotePath=" + remotepath.Text);
            }
        }
    }
}
