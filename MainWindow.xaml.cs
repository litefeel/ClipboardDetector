using ClipboardDetector.Providers;
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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SharpClipboard clipboard;
        private string filename = @"D:\work\warcommander\Src\trunk\Client\Assets\BuildOnlyAssets\Language\zh-CN.txt";
        private Dictionary<string, string> map = new Dictionary<string, string>();
        private StringBuilder buffer = new StringBuilder();
        private Config config;
        private string lastPath;

        private bool isSmall = false;
        private MyNotifyIcon notifyIcon;

        public MainWindow()
        {
            clipboard = new SharpClipboard();

            clipboard.ObservableFormats.All = false;
            clipboard.ObservableFormats.Texts = true;
            clipboard.ClipboardChanged += ClipboardChanged;

            Console.WriteLine("==================");

            InitializeComponent();

            isSmall = true;
            myWindow_MouseDoubleClick(null, null);

            notifyIcon = new MyNotifyIcon();
            notifyIcon.OnItemClick = ReloadItem;
            notifyIcon.ReloadDataClick = ReloadData;
            notifyIcon.QuitClick = MenuItem_Quit_Click;
            notifyIcon.NotifyIconDoubleClick = OnTrayIconDoubleClick;
            notifyIcon.Init();

            ReloadData();
        }

        private void ReloadItem(Item item)
        {
            Console.WriteLine(item.path);
            if (!File.Exists(item.path)) return;

            if (string.IsNullOrEmpty(item.format))
                item.format = Path.GetExtension(item.path).Substring(1).ToLower();

            Action<string, Dictionary<string, string>> func = item.format switch
            {
                "lua" => LoadDataByLua,
                "ini" => LoadDataByIni,
                "bytes" => ProviderBytes.Load,
                "xlsx" => ProviderExcel.Load,
                _ => null,
            };
            map.Clear();
            func?.Invoke(item.path, map);

            lastPath = item.path;


            UpdateClipboardText();
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
            config = ReadConfig();
            var itemIdx = 0;
            if (config != null && config.items != null && config.items.Count > 0)
            {
                Item item;
                for (var i = 0; i < config.items.Count; i++)
                {
                    item = config.items[i];
                    if (string.IsNullOrEmpty(item.name))
                        item.name = Path.GetFileNameWithoutExtension(item.path);
                    if (item.path == lastPath)
                        itemIdx = i;
                }

                item = config.items[itemIdx];
                ReloadItem(item);
            }
            notifyIcon.ResetItems(config?.items, itemIdx);
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
                if (line.StartsWith(KeyPrefix))
                {
                    key = line.Substring(KeyPrefix.Length, line.Length - KeyPrefix.Length - 2);
                }
                else if (line.StartsWith(ValuePrefix))
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

                //myTxt.Width = myTxt.ActualWidth;

                UpdateClipboardText();
            }
        }

        private void UpdateClipboardText()
        {
            if (string.IsNullOrEmpty(clipboard.ClipboardText))
                return;
            myTxt.Text = $"ClipboardChanged {clipboard.ClipboardText}";
            var text = Trim(clipboard.ClipboardText);
            if (map.TryGetValue(text, out var value))
            {
                myTxt.Text = value;
                if (isSmall)
                    myWindow_MouseDoubleClick(null, null);
            }
        }

        private void myWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

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

        private void MenuItem_Quit_Click()
        {
            clipboard.Dispose();
            Application.Current.Shutdown();
        }

        private void OnTrayIconDoubleClick()
        {
            isSmall = true;
            myWindow_MouseDoubleClick(null, null);
            Application.Current.MainWindow.Left = 100;
            Application.Current.MainWindow.Top = 100;
            Application.Current.MainWindow.Show();
        }
    }
}
