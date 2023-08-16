using DronDoomTexUtilsDLL;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace DronDoomTexUtils
{
    public class ToCSV_Data
    {
        public string WADFileFullPath;
        public string WADFileName;
        public string CSVBasicOutputPath;
        public string CSVOutputPath;
        public Logger? logger;

        public ToCSV_Data()
        {
            WADFileFullPath = "";
            WADFileName = "";
            CSVBasicOutputPath = "";
            CSVOutputPath = "";
            logger = null;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ToCSV_Data toCSV_data = new();
        private WAD? ToCSV_LoadedWAD = null;

        public MainWindow()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            Style = (Style)FindResource(typeof(Window));

            toCSV_data.logger = new Logger(new Logger.LogDelegate(ToCSV_LogLine));
        }

        #region Main window actions
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Normal)
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Normal;
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        #endregion

        #region To CSV Tab
        private void ToCSV_OpenWADButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".wad";
            openFileDialog.Filter = "Where All Data? (.wad)|*.wad";
            if (openFileDialog.ShowDialog() == true)
            {
                toCSV_data.WADFileFullPath = openFileDialog.FileName;

                toCSV_data.WADFileName = System.IO.Path.GetFileNameWithoutExtension(toCSV_data.WADFileFullPath);
            }

            ToCSV_ActualizeUI(toCSV_data);
        }

        private void ToCSV_OpenCSVPathButton_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog openFolderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (openFolderDialog.ShowDialog() == true)
                toCSV_data.CSVBasicOutputPath = openFolderDialog.SelectedPath;

            ToCSV_ActualizeUI(toCSV_data);
        }

        public void ToCSV_ActualizeUI(ToCSV_Data data)
        {
            ToCSV_WADInputPathTextbox.Text = data.WADFileFullPath;

            data.CSVOutputPath = data.CSVBasicOutputPath + '\\' + (data.WADFileName == null ? "output" : (data.WADFileName == "" ? "output" : data.WADFileName)) + "\\";
            ToCSV_CSVOutputPathTextbox.Text = data.CSVOutputPath;
        }

        public void ToCSV_LogLine(string message)
        {
            ToCSV_LogTextbox.Text += message + '\n';
        }
        #endregion

        private void ToCSV_ActionButton_Click(object sender, RoutedEventArgs e)
        {
            ToCSV_LoadedWAD?.Dispose();

            ToCSV_LoadedWAD = new WAD(toCSV_data.WADFileFullPath, toCSV_data.logger);
            Directory.CreateDirectory(toCSV_data.CSVOutputPath);
            toCSV_data.logger?.Log($"[{ToCSV_LoadedWAD.FileName}] Directory - {toCSV_data.CSVOutputPath}");
            toCSV_data.logger?.Log($"[{ToCSV_LoadedWAD.FileName}] PNAMES - {toCSV_data.CSVOutputPath + "PNAMES.csv"}");
            toCSV_data.logger?.Log($"[{ToCSV_LoadedWAD.FileName}] TEXTUREs - {toCSV_data.CSVOutputPath + "TEXTUREs.csv"}");
            toCSV_data.logger?.Log($"[{ToCSV_LoadedWAD.FileName}] TEXTUREs with PATСHES - {toCSV_data.CSVOutputPath + "TEXTUREs with PATСHES.csv"}");
            ToCSV_LoadedWAD.PNAMEStoCSV(toCSV_data.CSVOutputPath + "PNAMES.csv");
            ToCSV_LoadedWAD.TEXTUREStoCSV(toCSV_data.CSVOutputPath + "TEXTUREs.csv");
            ToCSV_LoadedWAD.TEXTUREwithPATCHEStoCSV(toCSV_data.CSVOutputPath + "TEXTUREs with PATСHES.csv");
            ToCSV_LoadedWAD.Close();
            ToCSV_LoadedWAD.Dispose();
        }
    }
}
