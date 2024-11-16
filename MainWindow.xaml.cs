using System.Windows;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpookyInjector
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FolderOpen_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Spooky");
        }

        private void InjectButton_Click(object sender, RoutedEventArgs e)
        {
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
    }
}