using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModeManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public static class CurrentPage
    {
        public static int number { get; set; } = 0;
    }

    public partial class MainWindow : Window
    {
        Storyboard storyboard = new Storyboard();
        TimeSpan duration = TimeSpan.FromMilliseconds(2000);
        DoubleAnimation fadeInAnimation;

        public int stage { get; set; } = 0;


        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(Directory.GetCurrentDirectory() + "\\LoginDetails.ini"))
            {
                FrmViewer.Navigate(new Uri("Pages/MainPage.xaml", UriKind.Relative));
            }
            else
            {
                FrmViewer.Navigate(new Uri("Pages/WelcomePage.xaml", UriKind.Relative));
            }

            NextPageEffect();
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseBtn(object sender, RoutedEventArgs e)
        {
            if (MainPage.temp_folder != null)
            {
                Directory.GetFiles(MainPage.temp_folder).ToList().ForEach(File.Delete);
                Directory.Delete(MainPage.temp_folder);
            }

            Close();
        }

        private void MinBtn(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        public void NextPageEffect()
        {
            fadeInAnimation = new DoubleAnimation() { From = 0, To = 1.0, Duration = new Duration(duration) };

            Storyboard.SetTargetName(fadeInAnimation, FrmViewer.Name);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity", 1));
            storyboard.Children.Add(fadeInAnimation);
            storyboard.Begin(FrmViewer);
        }
    }
}
