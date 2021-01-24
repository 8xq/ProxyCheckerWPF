using Microsoft.Win32;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Leaf.xNet;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace ProxyCheckerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region All of the variables needed for proxy checking / statistics reporting
        private static int MaxThreads = 500; // Maximum threads (conccurent checking)
        private static int ProxyTimeout = 7500; // Proxy timeout in milliseconds (MS)
        private static int ProgressTarget; // This is the amount of proxies loaded
        private static int BadProxies; // Bad proxies (not working) 
        private static int GoodProxies; // Good proxies (reply with info within MS timeout)
        private static int Progress; // Current amount of proxies checked 
        private static string ProxyType; // Set a proxytype as radio button is causing issues
        public static List<string> ProxyList = new List<string>(); // List containing imported proxies
        public static List<string> WorkingProxies = new List<string>(); // List containing working proxies
        #endregion
        #region initialize
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion
        #region clear listbox / reset all items
        private void ClearPXYbox_Click(object sender, RoutedEventArgs e)
        {
            ProxyBox.Items.Clear();
        }
        #endregion
        #region Load proxies button (and counter)
        private void LoadPXY_click(object sender, RoutedEventArgs e)
        {
            ProxyList.Clear();
            ProxiesLoadedCounter.Content = "0";
            ProgressTarget = 0;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                ProxyList.AddRange(File.ReadAllLines(openFileDialog.FileName));
                ProxiesLoadedCounter.Content = ProxyList.Count();
                PXYprogress.Maximum = ProxyList.Count();
                ProgressTarget = ProxyList.Count();
                MessageBox.Show("Imported a total of " + ProxyList.Count() + " Proxies", "Success - proxies imported");
            }
            else
            {
                MessageBox.Show("Error importing proxy list ! ", "Error");
            }
        }
        #endregion
        #region Save proxies button
        private void SaveProxies_click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt|C# file (*.cs)|*.cs";
            if (saveFileDialog.ShowDialog() == true)
            {
                using (var file = new StreamWriter(saveFileDialog.FileName))
                {
                    WorkingProxies.ForEach(proxy => file.WriteLine(proxy));
                }
                MessageBox.Show("Saving a total of " + WorkingProxies.Count() + " proxies", "Success");
            }
            else
            {
                MessageBox.Show("Error with file path ", "Error");
            }
        }
        #endregion
        #region Check proxy (main checking function)
        private void CheckProxy_click(object sender, RoutedEventArgs e)
        {
            #region clear values / reset
            WorkingProxies.Clear();
            PXYprogress.Value = 0;
            DeadCounter.Content = 0;
            WorkingCounter.Content = 0;
            ProgressCounter.Content = 0;
            BadProxies = 0;
            GoodProxies = 0;
            Progress = 0;
            #endregion
            #region disable buttons whilst running 
            ClearPXYbox.IsEnabled = false; // Disable the clear listbox button
            LoadPXY.IsEnabled = false; // Disable load proxy button until done
            SaveProxies.IsEnabled = false; // Disable the save proxies button until done
            RadioHTTP.IsEnabled = false; // Disable HTTP radio button
            RadioSocks4.IsEnabled = false; // Disable Socks4 radio button
            RadioSocks5.IsEnabled = false; // Disable socks5 radio button
            #endregion
            new Thread(() => {
                object sync = new Object();
                Parallel.ForEach(ProxyList, new ParallelOptions
                {
                    MaxDegreeOfParallelism = MaxThreads
                },
                (Proxy, _, lineNumber) => {
                    try
                    {
                        HttpRequest req = new HttpRequest();
                        req.IgnoreProtocolErrors = true;
                        req.ConnectTimeout = ProxyTimeout;
                        if(ProxyType.ToLower() == "http")
                        {
                            req.Proxy = HttpProxyClient.Parse(Proxy);
                        }
                        if(ProxyType.ToLower() == "socks4")
                        {
                            req.Proxy = Socks4ProxyClient.Parse(Proxy);
                        }
                        if(ProxyType.ToLower() == "socks5")
                        {
                            req.Proxy = Socks5ProxyClient.Parse(Proxy);
                        }
                        var ProxyInformation = req.Get("http://ip-api.com/json/"); // URL to grab information on IP / test request
                        string code = ProxyInformation.StatusCode.ToString().ToLower(); // Check status code for 200OK
                        JObject o = JObject.Parse(ProxyInformation.ToString()); // Parse json based on token
                        string Country = (o.SelectToken("country").ToString()); // Parse json "Country" selector 
                        string isp = (o.SelectToken("isp").ToString()); // Parse json "isp" selector 
                        string status = (o.SelectToken("status").ToString().ToLower()); // Parse json "status" selector 
                        if (status == "success" && code == "200" || code == "ok") // Only save proxy if response is 200OK & response body is a success
                        {
                            #region update proxy listbox and working proxy labels / counters
                            Dispatcher.Invoke(() => {
                                Interlocked.Increment(ref GoodProxies);
                                WorkingCounter.Content = GoodProxies.ToString();
                                WorkingProxies.Add(Proxy);
                                ProxyBox.Items.Add(Proxy + " | " + Country + " | " + isp);
                            });
                            #endregion
                        }
                        else
                        {
                            #region update bad proxy counters
                            Dispatcher.Invoke(() => {
                                Interlocked.Increment(ref BadProxies);
                                DeadCounter.Content = BadProxies.ToString();
                            });
                            #endregion
                        }
                    }
                    catch (Exception)
                    {
                        #region update bad proxy counters
                        Dispatcher.Invoke(() => {
                            Interlocked.Increment(ref BadProxies);
                            DeadCounter.Content = BadProxies.ToString();
                        });
                        #endregion
                    }
                    #region update the progress bar and "progress" counters
                    Dispatcher.Invoke(() => {
                        Interlocked.Increment(ref Progress);
                        ProgressCounter.Content = Progress.ToString() + "/" + ProgressTarget;
                        PXYprogress.Value = PXYprogress.Value + 1;
                    });
                    #endregion
                }); ; ;
                #region re enable buttons when finished
                Dispatcher.Invoke(() => {
                    LoadPXY.IsEnabled = true; // Re enable the load proxy button now its finished !
                    ClearPXYbox.IsEnabled = true; // Re enable the clear listbox button
                    SaveProxies.IsEnabled = true; // Re enable the save proxies button upon completion
                    RadioHTTP.IsEnabled = true; // Re enableHTTP radio button
                    RadioSocks4.IsEnabled = true; // Re enable Socks4 radio button
                    RadioSocks5.IsEnabled = true; // Re enable socks5 radio button
                });
                #endregion
            }).Start();
        }
        #endregion

        #region Set proxy type based on radio button click
        private void RadioHTTP_Checked(object sender, RoutedEventArgs e)
        {
            ProxyType = "http";
        }

        private void RadioSocks4_Checked(object sender, RoutedEventArgs e)
        {
            ProxyType = "socks4";
        }

        private void RadioSocks5_Checked(object sender, RoutedEventArgs e)
        {
            ProxyType = "socks5";
        }
        #endregion
    }
}