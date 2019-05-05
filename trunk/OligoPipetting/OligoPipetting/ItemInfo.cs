using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OligoPipetting
{
    enum ColumnIndexDefinition
    {
        odPerTube = 'H' - 'A' +1,
        srcPlateBarcode = 'K' - 'A' + 1,
        dstPlateBarcode = 'M' - 'A' + 1,
        srcWellID = 'L' -'A' +1,
        dstLabwareType = 'N'-'A'+1,
        dstWellID = 'O'-'A'+1,
        requireVolume = 'P'-'A'+1,  //pipettingVolume = requireVolume * orgVolumePerSlice / (od/tube)
        needMeasure = 'Q' - 'A' + 1,
        addWater2DiluteVolume = 'R' - 'A' + 1,
        orgVolumePerSlice = 'S' - 'A' + 1,
        needPipetting = 'T' - 'A' + 1,
        orgConcentration = 'U' - 'A' + 1,
        addWater4MeasureVolume = 'V' - 'A' + 1,
        addSample4MeasureVolume = 'W' - 'A' + 1,
        needNormalization = 'X' - 'A' + 1,
        orgSampleTotalVolume = 'Y' - 'A' + 1,
        dstConcentration = 'Z' - 'A' + 1,
        addWaterVolume = dstConcentration + 1, //'AA' = 'Z'+1
        maxNormalizationVolume = addWaterVolume + 1, //'AC' = 'Z'+3
        relativeMolecularMass = maxNormalizationVolume + 2, //AE
        concAfterDilution = maxNormalizationVolume +1,
//        judgeStandard = 'G' -'A' + 'Z' + 1
        //补水体积是根据原液浓度和目标浓度计算得到
        //测量是另外一块板，v,w,吸水体积，移液体积
    };

    public enum DstLabwareType
    {
        Well96 = 0,
        Well384,
        EppendorfTube
    };

    public class ItemInfo
    {
        public DstLabwareType dstLabwareType;
        public int srcWellID;
        public double odPerTube;
        public string srcPlateBarcode;
        public string dstPlateBarcode;

        public int dstWellID;
        public double requireVolume;  //pipettingVolume = requireVolume * orgVolumePerSlice / (od/tube)
        public bool needMeasure;
        public bool addWater2DiluteVolume;
        public double orgVolumePerSlice;
        public bool needPipetting;
        public double orgConcentration;
        public double addWater4MeasureVolume;
        public double addSample4MeasureVolume;
        public bool needNormalization;
        public double orgSampleTotalVolume;
        public double dstConcentration;
        public double maxNormalizationVolume;
        public double concAfterDilution;
        public double realWaterVolumeAdded4Dilution;
        public double relativeMolecularMass;
        public double firstTimeReaderResult;
        public double secondTimeReaderResult;
        //public double judgeStandard = 'G' - 'A' + 'Z' + 1;
    }

}
