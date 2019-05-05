using System.Collections.Generic;
using System.Configuration;

namespace OligoPipetting
{
    class GlobalVars
    {
        static GlobalVars instance;
        string workingFolder;
        int wasteGrid;
        int sampleGrid;
        int sampleSite;
        int uvGrid;
        int uvSite;

       
        string labwareType;
        string sampleLabwareLabel;
        string uvLabwareLabel;
        string resultFilePath;
        string dstLabwareLabel;

        public static GlobalVars Instance
        {
            get
            {
                if (instance == null)
                    instance = new GlobalVars();
                return instance;
            }
        }

        protected GlobalVars()
        {
            wasteGrid = int.Parse(ConfigurationManager.AppSettings["wasteGrid"]);
            sampleGrid = int.Parse(ConfigurationManager.AppSettings["sampleGrid"]);
            sampleSite = int.Parse(ConfigurationManager.AppSettings["sampleSite"]);
            uvGrid = int.Parse(ConfigurationManager.AppSettings["uvGrid"]);
            uvSite = int.Parse(ConfigurationManager.AppSettings["uvSite"]);
         
            labwareType = ConfigurationManager.AppSettings["labwareType"];
            sampleLabwareLabel = ConfigurationManager.AppSettings["sampleLabwareLabel"];
            uvLabwareLabel = ConfigurationManager.AppSettings["uvLabwareLabel"];
            dstLabwareLabel = ConfigurationManager.AppSettings["dstLabwareLabel"];
            workingFolder = ConfigurationManager.AppSettings["workingFolder"];
            resultFilePath = ConfigurationManager.AppSettings["resultFilePath"];
        }

        public int WasteGrid
        {
            get
            {
                return wasteGrid;
            }
        }

        public string LabwareType { get
            {
                return labwareType;
            }
        }

        public string DstLabwareLabel
        {
            get
            {
                return dstLabwareLabel;
            }
        }

        public string SampleLabwareLabel
        {
            get
            {
                return sampleLabwareLabel;
            }
        }

        public string UVLabwareLabel
        {
            get
            {
                return uvLabwareLabel;
            }
        }


        public int SampleGrid
        {
            get
            {
                return sampleGrid;
            }
        }

        public int SampleSite
        {
            get
            {
                return sampleGrid;
            }
        }

        public int UVGrid { get
            {
                return UVGrid;
            }
        }

        public int UVSite
        {
            get
            {
                return uvSite;
            }
        }

        public List<string> AllBarcodes { get; internal set; }
        public List<ItemInfo> ItemInfos { get; internal set; }
        public string WorkingFolder { get{ return workingFolder; } }

        public string ReaderResultFile { get { return resultFilePath; } }
    }
}