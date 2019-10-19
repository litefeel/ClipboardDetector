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
    }
}
