using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;

namespace SpookyInjector
{
    public partial class MainWindow : Window
    {
        string AutoInjectFile = "";
        bool AutoInjectEnabled = false;

        public MainWindow()
        {
            InitializeComponent();
            Thread BackgroundThread = new Thread(new ThreadStart(BackgroundLoop));
            BackgroundThread.IsBackground = true;
            BackgroundThread.Start();
        }

        private void FolderOpen_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Spooky");
        }

        private void ToggleAutoInject_Click(object sender, RoutedEventArgs e)
        {
            if (!AutoInjectEnabled)
            {
                OpenFileDialog OpenFileDialog = new OpenFileDialog();
                OpenFileDialog.Filter = "Dynamic Link Libraries (*.dll)|*.dll";
                OpenFileDialog.DefaultExt = ".dll";

                bool? Result = OpenFileDialog.ShowDialog();

                if (Result == true)
                {
                    AutoInjectFile = OpenFileDialog.FileName;
                    AutoInjectEnabled = true;
                    ToggleAutoInject.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#262626"));
                    MessageBox.Show("toggled on");
                }
            } else
            {
                AutoInjectFile = "";
                AutoInjectEnabled = false;
                ToggleAutoInject.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#363636"));
                MessageBox.Show("toggled off");
            }
        }

        private void InjectButton_Click(object sender, RoutedEventArgs e)
        {
            AutoInjectEnabled = false;
            OpenFileDialog OpenFileDialog = new OpenFileDialog();
            OpenFileDialog.Filter = "Dynamic Link Libraries (*.dll)|*.dll";
            OpenFileDialog.DefaultExt = ".dll";

            bool? Result = OpenFileDialog.ShowDialog();

            if (Result == true)
            {
                string Filename = OpenFileDialog.FileName;

                bool Success = false; int PID = 0;
                Helper.GetProcessIDByName(out Success, out PID, "Phasmophobia");
                if (Success)
                {
                    if (!Helper.InjectDLL(PID, Filename)) MessageBox.Show("Failed to inject");
                } else MessageBox.Show("Failed to find Phasmophobia, is the game running?");
            }
        }

        void BackgroundLoop()
        {
            while (true)
            {
                if (AutoInjectEnabled && AutoInjectFile.Length != 0)
                {
                    bool Success = false; int PID = 0;
                    Helper.GetProcessIDByName(out Success, out PID, "Phasmophobia");
                    if (Success) Thread.Sleep(5000);
                    Helper.GetProcessIDByName(out Success, out PID, "Phasmophobia"); // to make sure its still open
                    if (Success && AutoInjectEnabled)
                    {
                        Thread.Sleep(5000);
                        if (!Helper.InjectDLL(PID, AutoInjectFile)) MessageBox.Show("Failed to auto inject");
                        AutoInjectEnabled = false;
                    }
                }
                Thread.Sleep(500);
            }
        }
    }
}