using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using WK.Libraries.SharpClipboardNS;


namespace ClipboardDetector
{

    public class Config
    {
        public string path { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string StartUpKey = "ClipboardDetector";

        private SharpClipboard clipboard;
        private string filename = @"D:\work\warcommander\Src\trunk\Client\Assets\BuildOnlyAssets\Language\zh-CN.txt";
        private Dictionary<string, string> map = new Dictionary<string, string>();
        private StringBuilder buffer = new StringBuilder();


        public MainWindow()
        {
            clipboard = new SharpClipboard();

            clipboard.ObservableFormats.Texts = true;
            clipboard.ObservableFormats.Files = true;
            clipboard.ObservableFormats.Images = true;
            clipboard.ObservableFormats.Others = true;
            clipboard.ClipboardChanged += ClipboardChanged;

            Console.WriteLine("==================");

            InitializeComponent();

            isSmall = true;
            myWindow_MouseDoubleClick(null, null);

            ReloadData();

            AutoStartUpMenuItem.IsChecked = IsAutoStartUp();
        }

        private string GetCurrentPath()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var dir = Path.GetDirectoryName(assembly.Location);
            var filename = Path.Combine(dir, assembly.GetName().Name + ".exe");
            return filename;
        }

        private bool IsAutoStartUp()
        {
            string path = GetCurrentPath();
            RegistryKey rk = Registry.CurrentUser;
            RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            var oldPath = rk2.GetValue(StartUpKey) as String;
            rk2.Close();
            rk.Close();
            return oldPath == path;
        }

        private void ChangeAutoStartUp(bool isSet)
        {
            string path = GetCurrentPath();
            // 注意这里推荐使用 CurrentUser 而不是 LocalMachine
            // LocalMachine 需要注册权限
            RegistryKey rk = Registry.CurrentUser;
            RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if (isSet)
                rk2.SetValue(StartUpKey, path);
            else
                rk2.DeleteValue(StartUpKey, false);
            rk2.Close();
            rk.Close();
        }


        protected override void OnClosed(EventArgs e)
        {
            clipboard.Dispose();
            base.OnClosed(e);
        }

        private Config ReadConfig()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var dir = Path.GetDirectoryName(assembly.Location);
            var filename = Path.Combine(dir, assembly.GetName().Name + ".exe.json");
            if (!File.Exists(filename))
            {
                var json = JsonSerializer.Serialize(new Config());
                File.WriteAllText(filename, json);
                Application.Current.Shutdown();
                return null;
            }

            var data = File.ReadAllText(filename);

            try
            {
                var config = JsonSerializer.Deserialize<Config>(data);
                return config;
            }
            catch (JsonException ex)
            {
                return null;
            }
        }

        private void ReloadData()
        {
            map.Clear();
            var config = ReadConfig();
            var path = config?.path ?? filename;

            Console.WriteLine(path);
            if (!File.Exists(path)) return;

            LoadDataByLua(path, map);
        }

        private void LoadDataByLua(string path, Dictionary<string, string> map)
        {
            const string KeyPrefix = "key = \"";
            const string ValuePrefix = "ChineseSimplified = \"";
            var lines = File.ReadAllLines(path);
            string key = null;
            foreach (var fullline in lines)
            {
                var line = fullline.Trim();
                if(line.StartsWith(KeyPrefix))
                {
                    key = line.Substring(KeyPrefix.Length, line.Length - KeyPrefix.Length - 2);
                }else if(line.StartsWith(ValuePrefix))
                {
                    var value = line.Substring(ValuePrefix.Length, line.Length - ValuePrefix.Length - 2);
                    map[key] = $"{key} = {value}";
                }
            }
        }

        private void LoadDataByIni(string path, Dictionary<string, string> map)
        {
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;

                var arr = line.Split('"');
                if (arr.Length == 5)
                    map[arr[1]] = line;
            }
        }

        private string Trim(string str)
        {
            buffer.Clear();
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (c == ' ' || c == '"' || c == '(' || c == ')' || c == ';')
                    continue;
                buffer.Append(c);
            }
            return buffer.ToString().Trim();
        }

        private void ClipboardChanged(object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            myTxt.Text = $"ClipboardChanged {e.ContentType}";

            Console.WriteLine($"ClipboardChanged {e.ContentType}");
            // Is the content copied of text type?
            if (e.ContentType == SharpClipboard.ContentTypes.Text)
            {
                // Get the cut/copied text.
                Console.WriteLine(clipboard.ClipboardText);
                myTxt.Text = $"ClipboardChanged {e.ContentType} {clipboard.ClipboardText}";
                //myTxt.Width = myTxt.ActualWidth;



                var text = Trim(clipboard.ClipboardText);
                if (map.TryGetValue(text, out var value))
                {
                    myTxt.Text = value;
                    if (isSmall)
                        myWindow_MouseDoubleClick(null, null);
                }
            }

            // Is the content copied of image type?
            else if (e.ContentType == SharpClipboard.ContentTypes.Image)
            {

                // Get the cut/copied image.
                //var img = clipboard.ClipboardImage;
            }

            // Is the content copied of file type?
            else if (e.ContentType == SharpClipboard.ContentTypes.Files)
            {
                // Get the cut/copied file/files.
                //Debug.WriteLine(clipboard.ClipboardFiles.ToArray());

                // ...or use 'ClipboardFile' to get a single copied file.
                //Debug.WriteLine(clipboard.ClipboardFile);
            }

            // If the cut/copied content is complex, use 'Other'.
            else if (e.ContentType == SharpClipboard.ContentTypes.Other)
            {
                // Do something with 'clipboard.ClipboardObject' or 'e.Content' here...
            }
        }

        private void myWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();


        }

        private bool isSmall = false;
        private void myWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            isSmall = !isSmall;
            if (isSmall)
            {
                myWindow.Width = 50;
                myWindow.Height = 50;
                myWindow.Opacity = 0.5d;
            }
            else
            {
                myWindow.Width = 400;
                myWindow.Height = 70;
                myWindow.Opacity = 0.9d;
            }

        }

        private void MenuItem_Reload_Click(object sender, RoutedEventArgs e)
        {
            ReloadData();
        }
        private void MenuItem_Quit_Click(object sender, RoutedEventArgs e)
        {
            clipboard.Dispose();
            Application.Current.Shutdown();
        }

        private void MenuItem_AutoStartUp_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(AutoStartUpMenuItem.IsChecked);
            ChangeAutoStartUp(AutoStartUpMenuItem.IsChecked);
        }

        private void OnTrayIconDoubleClick(object sender, RoutedEventArgs e)
        {
            isSmall = true;
            myWindow_MouseDoubleClick(null, null);
            Application.Current.MainWindow.Left = 100;
            Application.Current.MainWindow.Top = 100;
            Application.Current.MainWindow.Show();
        }
    }
}
