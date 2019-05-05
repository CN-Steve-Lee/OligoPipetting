using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace OligoPipetting
{
    struct POINT
    {
        public double x;
        public double y;
        public POINT(double v1, double v2)
        {
            x = v1;
            y = v2;
        }
    };
    enum PlateType
    {
        Sample,
        UV, //for measure
    }

    class Worklist
    {
        const string systemLiquid = "System Liquid free dispense";


        List<string> GenerateMeasurement(List<ItemInfo> itemInfos)
        {
            if (itemInfos.Count(x => x.needMeasure) == 0)
                return new List<string>();

            List<string> strs = new List<string>();
            //add sample
            foreach(var itemInfo in itemInfos)
            {
                if (!itemInfo.needMeasure)
                    continue;
                double vol = itemInfo.addSample4MeasureVolume;
                string aspStr = GetAspirate(GlobalVars.Instance.SampleLabwareLabel, GlobalVars.Instance.LabwareType, itemInfo.srcWellID, vol);
                string dispStr = GetDispense(GlobalVars.Instance.UVLabwareLabel, GlobalVars.Instance.LabwareType, itemInfo.srcWellID, vol);
                strs.Add(aspStr);
                strs.Add(dispStr);
                strs.Add("W;");
            }

            //add water
            for(int col = 0; col < 12; col++)
            {
                var thisColumnItemInfos = itemInfos.Where(x => InColumn(x.srcWellID, col)).ToList();
                var addWaterStrs = AddWater(thisColumnItemInfos.Select(x => x.srcWellID).ToList(), thisColumnItemInfos.Select(x => x.addWater4MeasureVolume).ToList(), PlateType.UV);
                strs.AddRange(addWaterStrs);
            }
            return strs;
        }

        //96孔板样品原液浓度计算公式：=[（酶标仪测值2-酶标仪测值1）*1.78299*33*1000/引物相对分子量]*[（吸水体积 + 吸样品体积）/ 吸样品体积]
        //384孔板样品原液浓度计算公式：=[（酶标仪测值2-酶标仪测值1）*2.1885*33*1000/引物相对分子量]*[（吸水体积 + 吸样品体积）/ 吸样品体积]

        List<string> GenerateTransfer(List<ItemInfo> itemInfos)
        {
            if (itemInfos.Count(x => x.needPipetting) == 0)
                return new List<string>();

            List<string> strs = new List<string>();
            for(int i = 0; i< itemInfos.Count; i++) //use i for modify itemInfo in the loop
            {
               
                var itemInfo = itemInfos[i];
                if (!itemInfo.needPipetting)
                    continue;
                double vol = CalculateWaterVolume4Transfer(itemInfo);
                string aspStr = GetAspirate(GlobalVars.Instance.SampleLabwareLabel, GlobalVars.Instance.LabwareType, itemInfo.srcWellID, vol);
                string dispStr = GetDispense(GlobalVars.Instance.DstLabwareLabel, GlobalVars.Instance.LabwareType, itemInfo.dstWellID, vol);
                strs.Add(aspStr);
                strs.Add(dispStr);
                strs.Add("W;");
            }
            return strs;
        }

        internal void Generate4Plate(string sBarcode)
        {
            var thisBarcodeItemInfos = GlobalVars.Instance.ItemInfos.Where(x => x.srcPlateBarcode == sBarcode).ToList();
            
            var measureWklists =  GenerateMeasurement(thisBarcodeItemInfos);
            var normalizationWklists = GenerateNormalization(thisBarcodeItemInfos);
            var transferWklists = GenerateTransfer(thisBarcodeItemInfos);

            string outputFolder = Helper.GetOutputFolder();
            string measureDoOrNot = outputFolder + "measure.txt";
            File.WriteAllText(measureDoOrNot, (measureWklists.Count > 0).ToString());

            string normalizeDoOrNot = outputFolder + "normalize.txt";
            File.WriteAllText(normalizeDoOrNot, (normalizationWklists.Count > 0).ToString());

            string transferDoOrNot = outputFolder + "transfer.txt";
            File.WriteAllText(transferDoOrNot, (transferWklists.Count > 0).ToString());

            File.WriteAllLines(outputFolder + "measure.gwl", measureWklists);
            File.WriteAllLines(outputFolder + "normalize.gwl", normalizationWklists);
            File.WriteAllLines(outputFolder + "transfer.gwl", transferWklists);
        }

        List<string> GenerateNormalization(List<ItemInfo> itemInfos)
        {
            if (itemInfos.Count(x=>x.needNormalization) == 0)
                return new List<string>();
            //just add water
            List<string> strs = new List<string>();
            for (int col = 0; col < 12; col++)
            {
                var thisColumnItemInfos = itemInfos.Where(x => InColumn(x.srcWellID, col) && x.needNormalization).ToList();
                List<double> volumes = GetNormalizationVolumes(col,thisColumnItemInfos);
                var addWaterStrs = AddWater(thisColumnItemInfos.Select(x => x.srcWellID).ToList(), thisColumnItemInfos.Select(x => x.addWater4MeasureVolume).ToList(), PlateType.UV);
                strs.AddRange(addWaterStrs);
            }
            return strs;
        }

       

        private double CalculateWaterVolume4Normalization(ItemInfo itemInfo)
        {
            double maxWellVolume = itemInfo.maxNormalizationVolume;
            double expectedVolume = (itemInfo.orgConcentration / itemInfo.dstConcentration) * itemInfo.orgSampleTotalVolume;
            expectedVolume = Math.Min(maxWellVolume, expectedVolume);
            itemInfo.concAfterDilution = (itemInfo.orgSampleTotalVolume / expectedVolume) * itemInfo.orgConcentration;
            itemInfo.realWaterVolumeAdded4Dilution = expectedVolume - itemInfo.orgSampleTotalVolume;
            return itemInfo.realWaterVolumeAdded4Dilution;
        }

        
        private double CalculateWaterVolume4Transfer(ItemInfo thisWellItemInfo)
        {
            double pipettingVolume = thisWellItemInfo.requireVolume * thisWellItemInfo.orgVolumePerSlice / thisWellItemInfo.odPerTube;
            return pipettingVolume;
        }

    

        private List<double> GetNormalizationVolumes(int colIndex, List<ItemInfo> thisColumnItemInfos)
        {
            int start = colIndex * 8 + 1;
            int end = start + 8 - 1;
            List<double> volumes = new List<double>();
            for (int wellID = start; wellID <= end; wellID++)
            {

                bool hasThisWellInfo = thisColumnItemInfos.Exists(x => x.srcWellID == wellID);
                double vol = 0;
                if (hasThisWellInfo)
                {
                    var thisWellItemInfo = thisColumnItemInfos.Where(x => x.srcWellID == wellID).First();
                    vol = CalculateWaterVolume4Normalization(thisWellItemInfo);
                }

                volumes.Add(vol);
            }
            return volumes;
        }
       

       

        private bool InColumn(int srcWellID, int col)
        {
            int start = col * 8 + 1;
            int end = start + 8 - 1;
            return srcWellID >= start && srcWellID <= end;
        }

        private string GetAspirate(string sLabware, string labwareType, int srcWellID, double vol)
        {
            string sAspirate = string.Format("A;{0};;{1};{2};;{3:N1};;;",
                         sLabware, labwareType,
                         srcWellID,
                         vol);
            return sAspirate;
        }

        private string GetDispense(string sLabware, string labwareType, int dstWellID, double vol)
        {
            string sDispense = string.Format("D;{0};;{1};{2};;{3:N1};;;",
              sLabware, labwareType,
              dstWellID,
              vol);
            return sDispense;
        }



        List<string> AddWater(List<int> wellIDs, List<double> volumes, PlateType plateType )
        {
            var aspPts = GetAspPoints(volumes);
            string sAsp = GenerateAspirateCommand(aspPts, volumes, systemLiquid, -1, -1, 1, 8);

            var dispPts = GetDispPoints(wellIDs);
            int grid = plateType == PlateType.Sample ? GlobalVars.Instance.SampleGrid : GlobalVars.Instance.UVGrid;
            int site = plateType == PlateType.UV ? GlobalVars.Instance.SampleSite : GlobalVars.Instance.UVSite;
            string sDisp = GenerateDispenseCommand(dispPts, volumes, systemLiquid, grid, site, 12, 8);

            return new List<string>() { sAsp, sDisp };
        }

        List<string> AddTransfer(List<int> wellIDs, List<double> volumes, PlateType plateType)
        {
            var aspPts = GetAspPoints(volumes);
            string sAsp = GenerateAspirateCommand(aspPts, volumes, systemLiquid, -1, -1, 1, 8);

            var dispPts = GetDispPoints(wellIDs);
            int grid = plateType == PlateType.Sample ? GlobalVars.Instance.SampleGrid : GlobalVars.Instance.UVGrid;
            int site = plateType == PlateType.UV ? GlobalVars.Instance.SampleSite : GlobalVars.Instance.UVSite;
            string sDisp = GenerateDispenseCommand(dispPts, volumes, systemLiquid, grid, site, 12, 8);

            return new List<string>() { sAsp, sDisp };
        }

        private List<POINT> GetAspPoints(List<double> volumes)
        {
            List<POINT> pts = new List<POINT>();
            int tipID = 1;
            foreach (var volume in volumes)
            {
                if(volume != 0)
                    pts.Add(new POINT(1, tipID));
                tipID++;
            }
            return pts;
        }

        private List<POINT> GetDispPoints(List<int> wellIDs)
        {
            List<POINT> pts = new List<POINT>();
            foreach (var wellID in wellIDs)
            {
                int colIndex = (wellID - 1) / Common96.rowCnt;
                int rowIndex = wellID - colIndex * Common96.rowCnt - 1;
                pts.Add(new POINT(colIndex + 1, rowIndex + 1));
            }
            return pts;
        }
        

        private HashSet<int> GetTipIDs(List<double> volumes)
        {
            HashSet<int> tipIDs = new HashSet<int>();
            int tipID = 1;
            foreach (var volume in volumes)
            {
                if (volume != 0)
                    tipIDs.Add(tipID);
                tipID++;
            }
            return tipIDs;
        }

        public const string breakPrefix = "B;";
        public string GenerateAspirateCommand(List<POINT> wells, List<double> volumes, string liquidClass, int gridPos, int site, int width, int height)
        {
            return GenerateAspirateOrDispenseCommand(wells, volumes, liquidClass, gridPos, site, width, height, true);
        }

        public string GenerateDispenseCommand(List<POINT> wells, List<double> volumes, string liquidClass, int gridPos, int site, int width, int height)
        {
            return GenerateAspirateOrDispenseCommand(wells, volumes, liquidClass, gridPos, site, width, height, false);
        }
        protected int GetTipSelection(int samplesInTheBatch, int startTip = 0)
        {
            int tip = 0;
            for (int i = 0; i < samplesInTheBatch; i++)
                tip += (int)Math.Pow(2, i + startTip);
            return tip;
        }
        public static string Chr(int asciiCode)
        {
            if (asciiCode >= 0 && asciiCode <= 255)
            {
                byte[] byteArray = new byte[] { (byte)asciiCode };
                string strCharacter = System.Text.Encoding.Default.GetString(byteArray);
                return (strCharacter);
            }
            else
            {
                throw new Exception("ASCII Code is not valid.");
            }
        }



        protected int GetTipSelection(List<double> volumes)
        {
            int tip = 0;
            for (int i = 0; i < volumes.Count; i++)
            {
                if (volumes[i] == 0)
                    continue;
                tip += (int)Math.Pow(2, i);
            }
            return tip;
        }
        protected string GetWellSelection(int width, int height, List<POINT> wells)
        {
            string selString = string.Format("{0:X2}{1:X2}", width, height);
            int bitCounter = 0;
            int bitMask = 0;
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    bool bSel = wells.IndexOf(new POINT(x + 1, y + 1)) != -1;
                    if (bSel)
                        bitMask |= (1 << bitCounter);
                    if (++bitCounter > 6)
                    {
                        string tmpChar = Chr(48 + bitMask);
                        selString += tmpChar;
                        bitCounter = 0;
                        bitMask = 0;
                    }
                }
            }
            if (bitCounter > 0)
                selString += (char)('0' + bitMask);
            return selString;
        }


        protected string GenerateAspirateOrDispenseCommand(List<POINT> wells, List<double> volumes, string liquidClass, int gridPos, int site, int width, int height, bool aspirate)
        {
            //B; Aspirate(3, "Water free dispense", "20", "20", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 1, "0110300", 0, 0);
            int tipMask = GetTipSelection(volumes);
            List<POINT> not0Wells = new List<POINT>();
            for (int i = 0; i < wells.Count; i++)
            {
                if (volumes[i] != 0) //always start from 0 now,
                    not0Wells.Add(wells[i]);
            }
            string sVolumes = "";
            for (int i = 0; i < 12; i++)
            {
                string sTmp = "";
                if (i < volumes.Count) // has the volume
                    sTmp = string.Format("\"{0}\",", volumes[i]);
                else
                    sTmp = "0,";
                sVolumes += sTmp;
            }

            string sWellSelection = GetWellSelection(width, height, not0Wells);
            string sAspOrDis = aspirate ? "Aspirate" : "Dispense";
            return string.Format(breakPrefix + "{0}({1},\"{2}\",{3}{4},{5},1,\"{6}\", 0, 0);", sAspOrDis, tipMask, liquidClass, sVolumes, gridPos, site, sWellSelection);

        }
    }

    public class Helper
    {

        static string sSaveDate = "";
        public static void ResetSaveDate()
        {
            sSaveDate = "";
        }

        static public string GetSaveDateTime()
        {
            if (sSaveDate != "")
                return sSaveDate;

            sSaveDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            return sSaveDate;
        }
        public static string GetPosIDFolder()
        {
            string sSaveFolder = GetExeParentFolder() + "PosID\\";
            if (!Directory.Exists(sSaveFolder))
                Directory.CreateDirectory(sSaveFolder);
            return sSaveFolder;
        }

        public static string GetTempImageFolder()
        {
            string sImageFolder = GetExeParentFolder() + "Images\\";
            if (!Directory.Exists(sImageFolder))
                Directory.CreateDirectory(sImageFolder);
            string imgDir = sImageFolder + GetSaveDateTime();
            if (!Directory.Exists(sImageFolder))
                Directory.CreateDirectory(sImageFolder);
            return imgDir;
        }
        public static string GetTempFolder()
        {
            string sTempFolder = GetExeParentFolder() + "Temp\\";
            if (!Directory.Exists(sTempFolder))
                Directory.CreateDirectory(sTempFolder);
            return sTempFolder;
        }
        public static string GetSaveFolder()
        {
            string sSaveFolder = GetExeParentFolder() + "Data\\";
            if (!Directory.Exists(sSaveFolder))
                Directory.CreateDirectory(sSaveFolder);
            return sSaveFolder;
        }

        public static string GetWorklistFolder()
        {
            string sSaveFolder = GetExeParentFolder() + "Worklist\\";
            if (!Directory.Exists(sSaveFolder))
                Directory.CreateDirectory(sSaveFolder);
            return sSaveFolder;
        }

        static public string GetLatestImagePath(int cameraID)
        {

            bool bJpeg = ConfigurationManager.AppSettings["cameraVendor"] == "Do3Think";
            string file = GetExeParentFolder() + "Data\\latest";
            string suffix = bJpeg ? ".jpg" : ".bmp";
            string sCameraID = cameraID.ToString();
            return file + sCameraID + suffix;
        }
        static public string GetCalibFolder()
        {
            return GetExeParentFolder() + "Calib\\";
        }
        static public string GetExeParentFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            int index = s.LastIndexOf("\\");
            return s.Substring(0, index) + "\\";
        }

        static public string GetCameraSettingFilePath()
        {
            return GetExeFolder() + "cameraSettings.XML";
        }
        static public string GetExeFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return s + "\\";
        }

        static public string GetResultFile()
        {
            return GetExeFolder() + "result.txt";
        }

        public static string GetDate()
        {
            sSaveDate = DateTime.Now.ToString("yyyyMMdd");
            return sSaveDate + "\\";
        }

        internal static string GetOutputFolder()
        {
            string dir =  GetExeParentFolder() + "Output\\";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }
    }
}
