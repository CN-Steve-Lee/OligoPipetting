using System;
using System.Collections.ObjectModel;

namespace OligoPipetting
{
    internal class BarcodesViewModel : ViewModelBase
    {
        public ObservableCollection<string> expectedBarcodes;
        public ObservableCollection<string> scannedBarcodes = new ObservableCollection<string>();
        public ObservableCollection<string> missingBarcodes = new ObservableCollection<string>();
        public ObservableCollection<string> undefinedBarcodes = new ObservableCollection<string>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public BarcodesViewModel()
        {
        }

        public void AddScanBarcode(string s)
        {
            s = s.Trim();
            s = s.Replace("\r\n", "");
            s = s.ToUpper();
            int curBarcodesID = scannedBarcodes.Count + 1;
            log.InfoFormat("setindex : {0} for barcode {1}", curBarcodesID, s);

            if (!GlobalVars.Instance.AllBarcodes.Contains(s))
            {
                undefinedBarcodes.Add(string.Format("{0}: {1}", curBarcodesID, s));
            }

            if (scannedBarcodes.Contains(s))
                throw new Exception("barcode : " + s + "already exists!");

            
            scannedBarcodes.Add(string.Format("{0}: {1}", curBarcodesID, s));
            bool bok = missingBarcodes.Remove(s);
        }
    }
}