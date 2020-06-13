using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModeManager
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            InitializeComponent();
        }

        private void WelcomeGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).FrmViewer.Navigate(new Uri("Pages/SettingsPage.xaml", UriKind.Relative));
        }
    }
}
