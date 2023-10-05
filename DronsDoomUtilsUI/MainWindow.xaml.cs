using DronDoomTexUtilsDLL;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class WADItem : IDisposable
    {
        // Variables
        private bool isDisposed = false;

        private string wadFileFullPath;
        private string wadFileName;
        public WAD? WADFile;

        // Properties
        public string WADFileFullPath { get => wadFileFullPath; set => wadFileFullPath = value; }
        public string WADFileName { get => wadFileName; set => wadFileName = value; }

        // Constructors
        public WADItem()
        {
            wadFileFullPath = "";
            wadFileName = "";
            WADFile = null;
        }

        public WADItem(string wadfilefullname, string wadfilename)
        {
            wadFileFullPath = wadfilefullname;
            wadFileName = wadfilename;
            WADFile = null;
        }

        // Cleaning memory
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;
            if (disposing)
            {
                WADFile?.Dispose();
            }

            isDisposed = true;
        }

        ~WADItem()
        {
            Dispose(false);
        }
    }

    public class ToCSV_Data : IDisposable
    {
        // Variables
        private bool isDisposed = false;

        public ObservableCollection<WADItem>? WADItems { get; set; }
        public string CSVBasicOutputPath;
        public string CSVOutputPath;
        public Logger? logger;

        // Constructor
        public ToCSV_Data()
        {
            WADItems = new ObservableCollection<WADItem>();
            CSVBasicOutputPath = "";
            CSVOutputPath = "";
            logger = null;
        }

        // Cleaning memory
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;
            if (disposing)
            {
                if (WADItems != null) foreach (WADItem wadItem in WADItems) wadItem.Dispose();

                logger = null;
            }

            isDisposed = true;
        }

        ~ToCSV_Data()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ToCSV_Data toCSV_data = new();

        public MainWindow()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            Style = (Style)FindResource(typeof(Window));

            toCSV_data.logger = new Logger(new Logger.LogDelegate(ToCSV_LogLine));

            ToCSV_WADInputPathListbox.ItemsSource = toCSV_data.WADItems;
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

        public void ToCSV_LogLine(string message)
        {
            ToCSV_LogTextbox.Text += message + '\n';
        }
        #endregion

        private void ToCSV_ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (toCSV_data.CSVBasicOutputPath == null || toCSV_data.CSVBasicOutputPath.Trim(' ') == "")
            {
                toCSV_data.logger?.Log("Empty CSV output path!");
                return;
            }

            if (toCSV_data.WADItems != null && toCSV_data.WADItems.Count > 0)
            {
                foreach (WADItem waditem in toCSV_data.WADItems)
                {
                    try
                    {
                        waditem.WADFile?.Dispose();

                        waditem.WADFile = new WAD(waditem.WADFileFullPath, toCSV_data.logger);
                        string currentCSVOutput = toCSV_data.CSVBasicOutputPath + '/' + waditem.WADFileName + '/';
                        Directory.CreateDirectory(currentCSVOutput);
                        toCSV_data.logger?.Log($"[{waditem.WADFile.FileName}] Directory - {currentCSVOutput}");
                        waditem.WADFile?.PNAMEStoCSV(currentCSVOutput + "PNAMES.csv");
                        waditem.WADFile?.TEXTUREStoCSV(currentCSVOutput + "TEXTUREs.csv");
                        waditem.WADFile?.TEXTUREwithPATCHEStoCSV(currentCSVOutput + "TEXTUREs with PATСHES.csv");
                        waditem.WADFile?.FLATStoCSV(currentCSVOutput + "Flats.csv");
                    }
                    catch
                    {
                        toCSV_data.logger?.Log($"Error, when do something with {waditem?.WADFileName}");
                    }
                    finally
                    {
                        waditem.WADFile?.Close();
                        waditem.WADFile?.Dispose();
                    }
                }
            }
            else
            {
                toCSV_data.logger?.Log("No WAD files!");
            }
        }

        private void ToCSV_AddWADButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.DefaultExt = ".wad";
            openFileDialog.Filter = "Where All Data? (.wad)|*.wad";
            if (openFileDialog.ShowDialog() == true)
            {
                for (int i = 0; i < openFileDialog.FileNames.Length; i++)
                {
                    if (File.Exists(openFileDialog.FileNames[i]))
                    {
                        toCSV_data.WADItems?.Add(new WADItem(openFileDialog.FileNames[i], openFileDialog.SafeFileNames[i]));
                    }
                }
            }
        }

        private void ToCSV_DeleteWADButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = ToCSV_WADInputPathListbox.SelectedItem;

            if (selectedItem != null && selectedItem is WADItem selectedWADItem)
            {
                toCSV_data.WADItems?.Remove(selectedWADItem);
                selectedWADItem.Dispose();
            }
        }

        private void ToCSV_ClearWADsButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < toCSV_data.WADItems?.Count; i++) toCSV_data.WADItems[i].Dispose();

            toCSV_data.WADItems = new ObservableCollection<WADItem>();
            ToCSV_WADInputPathListbox.ItemsSource = toCSV_data.WADItems;
        }

        private void ToCSV_OpenCSVPathButton_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog openFolderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (openFolderDialog.ShowDialog() == true)
            {
                toCSV_data.CSVBasicOutputPath = openFolderDialog.SelectedPath;
                ToCSV_CSVOutputPathTextbox.Text = openFolderDialog.SelectedPath;
            }
        }
    }
}
