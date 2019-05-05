using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Utility;

namespace OligoPipetting
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        BarcodesViewModel barcodesVM = new BarcodesViewModel();
        string currentBarcode = "";
        public MainWindow()
        {
            InitializeComponent();
            lstBarcodes.DataContext = barcodesVM.scannedBarcodes;
            lstMissingBarcodes.DataContext = barcodesVM.missingBarcodes;
            lstUndefinedBarcodes.DataContext = barcodesVM.undefinedBarcodes;
            txtScaned.DataContext = barcodesVM.scannedBarcodes;
            txtMissing.DataContext = barcodesVM.missingBarcodes;
            txtUndefined.DataContext = barcodesVM.undefinedBarcodes;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Pipeserver.Close();
        }

        private void CreateNamedPipeServer()
        {
            Pipeserver.owner = this;
            Pipeserver.ownerInvoker = new Invoker(this);
            ThreadStart pipeThread = new ThreadStart(Pipeserver.createPipeServer);
            Thread listenerThread = new Thread(pipeThread);
            listenerThread.SetApartmentState(ApartmentState.STA);
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

        internal void ExecuteCommand(string sMessage)
        {
            string[] strs = sMessage.ToLower().Split(',');
            string sCommand = strs[0];
            if (sCommand == "barcode")
            {
                currentBarcode = strs[1];
                
                try
                {
                    barcodesVM.AddScanBarcode(currentBarcode);
                    Worklist wklist = new Worklist();
                    wklist.Generate4Plate(currentBarcode);
                }
                catch(Exception ex)
                {
                    SetInfo(ex.Message, true);
                }
            }
            else if(sCommand == "readResult")
            {
                sCommand = sCommand.Replace("readResult", "");
                if(int.Parse(sCommand) == 1) //first time
                {
                    MicroplateReader microPlateReader = new MicroplateReader();
                    microPlateReader.Read(currentBarcode);
                }
            }
        }

        void SetInfo(string s, bool error = false)
        {
            txtInfo.Foreground = error ? Brushes.Red : Brushes.Black;
            txtInfo.Text = s;
        }

        private void btnBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = ".xlsx";
            ofd.Filter = "excel file|*.xlsx";
            ofd.InitialDirectory = GlobalVars.Instance.WorkingFolder;
            if (ofd.ShowDialog() == true)
            {
                string file = ofd.FileName;
                LoadExcel(file);
            }
        }

        private void LoadExcel(string file)
        {
            try
            {
                var strs = ExcelHelper.ReadExcel(file);
                OperationSheet opSheet = new OperationSheet(strs);
                var itemInfos = opSheet.GetItemInfos();
                GlobalVars.Instance.ItemInfos = itemInfos;
                var barcodes = itemInfos.Select(x => x.srcPlateBarcode).ToList();
                barcodesVM.expectedBarcodes = new System.Collections.ObjectModel.ObservableCollection<string>(barcodes);
                barcodesVM.missingBarcodes = new System.Collections.ObjectModel.ObservableCollection<string>(barcodes);
                barcodesVM.undefinedBarcodes = new System.Collections.ObjectModel.ObservableCollection<string>();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message, true);
            }
            
        }

      
    }
}
