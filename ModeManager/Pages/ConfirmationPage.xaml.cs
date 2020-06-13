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
    /// Interaction logic for Page3.xaml
    /// </summary>
    public partial class ConfirmationPage : Page
    {
        public ConfirmationPage()
        {
            InitializeComponent();

            var bc = new BrushConverter();
            (Application.Current.MainWindow as MainWindow).MainBackground.Background = (Brush)bc.ConvertFrom("#12C06A");
        }

        private void EventMouseUp(object sender, MouseButtonEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).FrmViewer.Navigate(new Uri("Pages/MainPage.xaml", UriKind.Relative));
            (Application.Current.MainWindow as MainWindow).NextPageEffect();
        }
    }
}
