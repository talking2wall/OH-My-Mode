using FluentFTP;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ModeManager
{
    /// <summary>
    /// Interaction logic for Page4.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public class Plugin
        {
            public int Id { get; set; }
            public string Link { get; set; }
            public string Name { get; set; }
            public string Author { get; set; }
            public string Icon { get; set; }
        }

        // About Button Change Color Timer
        DispatcherTimer AboutColorChanger = new DispatcherTimer();
        float progress = 0;
        bool reverse_color = false;


        // SnackBar
        Snackbar sb = new Snackbar();
        SnackbarMessageQueue mq = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(3000));


        // Console Dots Effect Timer
        DispatcherTimer dotTimer = new DispatcherTimer();

        
        // Collection For Plugins List
        ObservableCollection<Plugin> Plugins_List = new ObservableCollection<Plugin>();


        // List of Tasks-Source-cancelletion-tokens
        List<CancellationTokenSource> cts_list = new List<CancellationTokenSource>();


        // Random Variables
        int isInstalling = 0;
        string hostname = "";
        string username = "";
        string password = "";
        string remotepath = "";
        public static string temp_folder = null;

        public MainPage()
        {
            InitializeComponent();

            // Change back the Grid Background Color
            var bc = new BrushConverter();
            (Application.Current.MainWindow as MainWindow).MainBackground.Background = (Brush)bc.ConvertFrom("#FF212121");


            // Load Plugins List
            PluginListAdd();


            // Initialize SnackBar
            MainGrid.Children.Add(sb);
            sb.MessageQueue = mq;
            sb.VerticalAlignment = VerticalAlignment.Bottom;
            sb.Margin = new Thickness(0, 0, 0, 20);


            // About Button Automatic Color Chaange
            AboutColorChanger.Interval = new TimeSpan(0, 0, 0, 0, 100);
            AboutColorChanger.Tick += new EventHandler(AboutColorChanger_Tick);
            AboutColorChanger.Start();
            AboutBtn.Background = new SolidColorBrush(Colors.Transparent);


            // Initialize Installing Timer Event
            dotTimer.Tick += new EventHandler(dotTimer_Tick);
            dotTimer.Interval = new TimeSpan(0, 0, 1);
        }

        
        // Fix "index out of bounds" error when search is made
        private void dgPlugins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Plugin selectedPlugin = dgPlugins.SelectedItem as Plugin;

            if (dgPlugins.SelectedIndex > -1)
            {
                PluginName.Text = selectedPlugin.Name;
                PluginSite.Text = selectedPlugin.Link;
            }
        }


        // Plugin Installation method
        private void DoInstallation(CancellationTokenSource cts, bool InstallAndStart = false, bool isSafeInstallation = false)
        {
            InstallationConsole.Visibility = Visibility.Visible;
            Console.Document.Blocks.Clear();
            DoResizeEffect();
            dotTimer.Start();

            string link = PluginSite.Text;

            CancellationToken ct = new CancellationToken();
            ct = cts.Token;

            try
            {
                Task.Run(() =>
                {
                    if (InstallAndStart && isSafeInstallation)
                    {
                        AppendText("[ Safe: Installation & Registeration ]\r", "LightGreen", !cts.IsCancellationRequested);
                    }
                    else if (InstallAndStart)
                    {
                        AppendText("[ None-Safe: Installation & Registeration ]\r", "Firebrick", !cts.IsCancellationRequested);
                    }
                    else
                    {
                        AppendText("[ Only-Installation ]\r", "LightYellow", !cts.IsCancellationRequested);
                    }

                    // GETTING HTML SCRIPT
                    string HtmlScript = SwitchToPostOne(link);
                    AppendText("Getting HTML Script.", "Red", !cts.IsCancellationRequested);
                    HtmlScript = GetHtmlScript(HtmlScript + "&postcount=1");
                    AppendText(" done.\r", "Green", !cts.IsCancellationRequested);
                    Debug.Print("SCRIPT");


                    if (cts.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }


                    // MAKING WORKING AREA
                    AppendText("Making working area.", "Red", !cts.IsCancellationRequested);
                    string working_area = MakeWorkingArea(HtmlScript);
                    AppendText(" done.\r", "Green", !cts.IsCancellationRequested);
                    Debug.Print("MAKING WORKING AREA", !cts.IsCancellationRequested);


                    if (cts.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }


                    // SEARCING FILES
                    AppendText("Searching for files.", "Red", !cts.IsCancellationRequested);
                    int files_count = CountFiles(working_area);
                    if (files_count <= 0)
                    {
                        AppendText(files_count + "\rInstallation aborted - No files have been found.\r", "Orange", !cts.IsCancellationRequested);
                        return;
                    }
                    string[] dl_urls = new string[files_count];
                    string[] file_names = new string[files_count];
                    dl_urls[0] = FindFile(working_area, "<a href=\"", "\"><strong>");
                    dl_urls[1] = "https://forums.alliedmods.net/attachment.php?attachmentid=" + FindFile(working_area, "attachmentid=", "&amp;d");
                    file_names[0] = GetFileName(dl_urls[0]);
                    file_names[1] = GetFileName(dl_urls[1]);
                    AppendText(" done.\r", "Green", !cts.IsCancellationRequested);
                    AppendText("Found " + files_count + " files:\r", "Orange", !cts.IsCancellationRequested);
                    AppendText(file_names[0] + "\r", "Yellow", !cts.IsCancellationRequested);
                    AppendText(file_names[1] + "\r", "Yellow", !cts.IsCancellationRequested);
                    Debug.Print("SEARCING FILES");



                    if (cts.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }


                    // DOWDLOADING FILES
                    AppendText("Downloading files.", "Red", !cts.IsCancellationRequested);
                    GetTemporaryDirectory();
                    DownloadFile(dl_urls[0]);
                    DownloadFile(dl_urls[1]);
                    AppendText(" done.\r", "Green", !cts.IsCancellationRequested);
                    Debug.Print("DOWDLOADING FILES", !cts.IsCancellationRequested);



                    if (cts.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }


                    // UPLOADING FILES
                    AppendText("Uploading Files.", "Red", !cts.IsCancellationRequested);
                    GetCredentials();
                    FtpClient client = new FtpClient();
                    client.Host = hostname;
                    client.Credentials = new NetworkCredential(username, password);
                    client.DataConnectionConnectTimeout = 10000;
                    client.Connect();
                    client.UploadFile(temp_folder + "\\" + file_names[0], remotepath + "addons\\amxmodx\\plugins\\" + file_names[0], FtpRemoteExists.Skip, true);
                    client.UploadFile(temp_folder + "\\" + file_names[1], remotepath + "addons\\amxmodx\\scripting\\" + file_names[1], FtpRemoteExists.Skip, true);
                    if (InstallAndStart)
                    {
                        if (isSafeInstallation)
                        {
                            string cfg_file_name = CreateConfigFile(file_names[0]);
                            client.UploadFile(temp_folder + "\\" + cfg_file_name, remotepath + "addons\\amxmodx\\configs\\" + cfg_file_name, FtpRemoteExists.Skip, true);
                        }
                        else
                        {
                            string plugin_name = file_names[0];
                            client.Connect();
                            using (Stream ostream = client.OpenAppend(remotepath + "addons\\amxmodx\\configs\\plugins.ini"))
                            {
                                var sr = new StreamWriter(ostream);
                                sr.Write("\n" + plugin_name + "\t\t; Generated by Oh-My-Mode");
                                sr.Flush();
                                ostream.Close();
                                if (client.GetReply().ErrorMessage.Length > 0)
                                {
                                    AppendText(files_count + "\rInstallation aborted - Failed to update plugins.ini file.\r", "Orange", !cts.IsCancellationRequested);
                                    return;
                                }
                            }
                        }
                    }
                    AppendText(" done.", "Green", !cts.IsCancellationRequested);
                    Debug.Print("UPLOADING FILES");



                    if (cts.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }


                    // FINISH
                    AppendText("\nFiles uploaded successfuly!" + "\r", "Green", !cts.IsCancellationRequested);
                    dotTimer.Stop();
                    Debug.Print("FINISH");
                    cts_list.Clear();
                }, ct);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        #region Buttons

        // Install Button
        private void InstallBtn_Click(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts_list.Add(cts);
            DoInstallation(cts, false);
        }

        // Install & Register - Button
        private void InstallAndStartBtn_Click(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts_list.Add(cts);
            DoInstallation(cts, true, (bool)SafeInstallationCb.IsChecked);
        }

        // Close Console - Button
        private void CloseConsoleBtn_Click(object sender, RoutedEventArgs e)
        {
            //tokenSource.Cancel();

            foreach (CancellationTokenSource cts in cts_list)
            {
                cts.Cancel();
            }

            dotTimer.Stop();
            InstallationConsole.Visibility = Visibility.Collapsed;
        }

        // About Button
        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.Show();
        }

        // Open Lisk - Button
        private void OpenLinkBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process myProcess = new Process();
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = PluginSite.Text;
                myProcess.Start();
            }
            catch (InvalidOperationException)
            {
                sb.MessageQueue.Enqueue("                        .לא נבחר מוד, בחר מוד מהרשימה");
            }
        }

        // Search Button
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            ListCollectionView collectionView = new ListCollectionView(Plugins_List);
            collectionView.Filter = (e) =>
            {
                Plugin temp = e as Plugin;
                if (temp.Author.ToLower().Contains(SearchTx.Text.ToLower()) ||
                    temp.Name.ToLower().Contains(SearchTx.Text.ToLower()))
                    return true;
                return false;
            };
            dgPlugins.ItemsSource = collectionView;
            
            // clear last selection
            dgPlugins.SelectedItem = null;
            PluginName.Text = "";
            PluginSite.Text = "";
        }

        // EnterClick-event
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchBtn_Click(sender, e);
            }
        }

        #endregion


        #region Functions

        // Reads PluginList.txt and adds the plugins accordingly
        private void PluginListAdd()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ModeManager.Assets.PluginsList.txt"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    int i = 1;

                    while (!reader.EndOfStream)
                    {
                        Plugins_List.Add(new Plugin() { Id = i++, Link = reader.ReadLine(), Name = reader.ReadLine(), Author = reader.ReadLine(), Icon = "../Assets/Images/amxx_icon.png" });
                        reader.ReadLine(); // skip empty line
                    }
                }
            }

            dgPlugins.ItemsSource = Plugins_List;
        }


        // Print given text in consoole in specificied color
        private void AppendText(string text, string color, bool shouldPrint = true)
        {
            // Executes the specified Action synchronously on the thread the Dispatcher is associated with.
            Application.Current.Dispatcher.Invoke(() => 
            {
                if (shouldPrint)
                {
                    BrushConverter bc = new BrushConverter();
                    TextRange tr = new TextRange(Console.Document.ContentEnd, Console.Document.ContentEnd);
                    tr.Text = text;
                    try
                    {
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, bc.ConvertFromString(color));
                    }
                    catch (FormatException) { }
                }
            });
        }


        // Generates temporary folder
        public void GetTemporaryDirectory()
        {
            string tempDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            temp_folder = tempDirectory;
        }


        // Read FTP credentials from LoginDetails.ini
        public void GetCredentials()
        {
            string path = Directory.GetCurrentDirectory();

            if (Directory.Exists(path))
            {
                string[] lines = File.ReadLines(path + "\\" + "LoginDetails.ini").ToArray();

                hostname = lines[1].Substring(5);
                username = lines[2].Substring(9);
                password = lines[3].Substring(9);
                remotepath = lines[4].Substring(11);

                if (remotepath.LastIndexOf("/") != remotepath.Length - 1)
                {
                    remotepath += "/";
                }
            }
            else
            {
                isInstalling++;
                dotTimer.Stop();
                AppendText("\nError: " + "LoginDetails.ini can't be found.\rPlease reset the program.", "Red");
            }
        }


        // Returns the url of post#1 from the Link
        private string SwitchToPostOne(string url)
        {
            return url.Replace("thread", "post");
        }


        // Get HTML-script of given url
        public string GetHtmlScript(string url)
        {
            string data = "";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;

                    if (String.IsNullOrWhiteSpace(response.CharacterSet))
                        readStream = new StreamReader(receiveStream);
                    else
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                    data = readStream.ReadToEnd();

                    response.Close();
                    readStream.Close();
                }
            }
            catch { }

            return data;
        }


        // Returns a string contains the relevent part of a HTML-script (all download links)
        private string MakeWorkingArea(string HtmlScript)
        {
            string working_area = "";

            int start_index = HtmlScript.IndexOf("<fieldset class=\"fieldset\">");
            working_area = HtmlScript.Substring(start_index);

            int stop_index = working_area.IndexOf("</fieldset>");
            working_area = working_area.Remove(stop_index);

            return working_area;
        }


        // Returns the file name of non-direct download link
        private string GetFileName(string url)
        {
            string fn = null;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                fn = response.Headers["Content-Disposition"].Split(new string[] { "=" }, StringSplitOptions.None)[1];
                fn = fn.Replace("\"", "");
            }

            return fn;
        }


        // Return the number of links (stating by "href") in a given string
        private int CountFiles(string working_area)
        {
            return Regex.Matches(working_area, "href").Count;
        }


        // Return links from string by given patterns
        private string FindFile(string working_area, string pattern_start, string pattern_stop, int remove_offset = 0)
        {
            int start_index = working_area.IndexOf(pattern_start);
            working_area = working_area.Substring(start_index + pattern_start.Length - remove_offset);

            int stop_index = working_area.IndexOf(pattern_stop);
            working_area = working_area.Remove(stop_index);

            return working_area;
        }


        // Download file from non-direct link to temp_folder
        private void DownloadFile(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                var fn = response.Headers["Content-Disposition"].Split(new string[] { "=" }, StringSplitOptions.None)[1];
                fn = fn.Replace("\"", "");
                var responseStream = response.GetResponseStream();
                using (var fileStream = File.Create(System.IO.Path.Combine(temp_folder, fn)))
                {
                    responseStream.CopyTo(fileStream);
                }
            }
        }


        // Creates a new config file to register a plugin in ftp
        // Returns the the generated name of the config file
        private string CreateConfigFile(string PluginName)
        {
            string cfg_file_name = "plugins-" + PluginName.Remove(PluginName.Length - 5) + ".ini";

            using (StreamWriter sw = File.CreateText(temp_folder + "\\" + cfg_file_name))
            {
                sw.WriteLine("; File Generated by Oh-My-Mode");
                sw.WriteLine(PluginName);
            }

            return cfg_file_name;
        }

        #endregion


        #region Effects

        // About Button color-change Timer
        private void AboutColorChanger_Tick(object sender, EventArgs e)
        {
            AboutBtn.Foreground = new SolidColorBrush(Rainbow(progress));

            if (progress >= 1.0)
            {
                reverse_color = true;
            }
            else if (progress <= 0.0)
            {
                reverse_color = false;
            }

            if (reverse_color)
            {
                progress -= (float)0.01;
            }
            else
            {
                progress += (float)0.01;
            }
        }


        // Bitterblue solution
        // https://stackoverflow.com/questions/2288498/how-do-i-get-a-rainbow-color-gradient-in-c
        private Color Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;

            switch ((int)div)
            {
                case 0:
                    return Color.FromArgb(255, 255, (byte)ascending, 0);
                case 1:
                    return Color.FromArgb(255, (byte)descending, 255, 0);
                case 2:
                    return Color.FromArgb(255, 0, 255, (byte)ascending);
                case 3:
                    return Color.FromArgb(255, 0, (byte)descending, 255);
                case 4:
                    return Color.FromArgb(255, (byte)ascending, 0, 255);
                default: // case 5:
                    return Color.FromArgb(255, 255, 0, (byte)descending);
            }
        }


        // Adds '.' to console (progress indicator) 
        private void dotTimer_Tick(object sender, EventArgs e)
        {
            AppendText(".", "Red");
        }


        // Create InstallationConsole Grow Effect
        private void DoResizeEffect()
        {
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = TimeSpan.FromMilliseconds(500);

            DoubleAnimation WidthEffect = new DoubleAnimation() { From = 0, To = 798, Duration = new Duration(duration) };
            storyboard.Children.Add(WidthEffect);
            Storyboard.SetTargetProperty(WidthEffect, new PropertyPath("Width", 1));

            DoubleAnimation HeightEffect = new DoubleAnimation() { From = 0, To = 418, Duration = new Duration(duration) };
            storyboard.Children.Add(HeightEffect);
            Storyboard.SetTargetProperty(HeightEffect, new PropertyPath("Height", 1));

            storyboard.Begin(InstallationConsole);
        }

        #endregion
    }
}
