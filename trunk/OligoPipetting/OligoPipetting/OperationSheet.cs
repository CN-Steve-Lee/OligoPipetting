using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OligoPipetting
{
    class OperationSheet
    {
        List<List<string>> allRowStrs;
        public OperationSheet(List<List<string>> allRowStrs)
        {
            this.allRowStrs = allRowStrs;
        }
        public List<ItemInfo> GetItemInfos()
        {

            List<ItemInfo> itemInfos = new List<ItemInfo>();
            List<string> meaningfulStrs = new List<string>();

            int lineIndex = 0;
            foreach (var thisRowStrs in allRowStrs)
            {
                //PlasmidInfo plasmidInfo = new PlasmidInfo();
                //PrimerInfo primerInfo = new PrimerInfo();
                ItemInfo itemInfo = new ItemInfo();
                try
                {
                    ParseRow(thisRowStrs, ref itemInfo);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("line {0} has error! ", lineIndex + 2) + ex.Message);
                }

                itemInfos.Add(itemInfo);
                lineIndex++;
            }
            return itemInfos;
        }

        private void ParseRow(List<string> thisRowStrs, ref ItemInfo itemInfo)
        {
            itemInfo.odPerTube = double.Parse(thisRowStrs[(int)ColumnIndexDefinition.odPerTube]);
            itemInfo.srcPlateBarcode = thisRowStrs[(int)ColumnIndexDefinition.srcPlateBarcode];
            itemInfo.dstPlateBarcode = thisRowStrs[(int)ColumnIndexDefinition.dstPlateBarcode];
            itemInfo.srcWellID = int.Parse(thisRowStrs[(int)ColumnIndexDefinition.srcWellID]);
            itemInfo.dstLabwareType = GetDstLabwareType(thisRowStrs[(int)ColumnIndexDefinition.dstLabwareType]);
            itemInfo.dstWellID = int.Parse(thisRowStrs[(int)ColumnIndexDefinition.dstWellID]);
            itemInfo.requireVolume = double.Parse(thisRowStrs[(int)ColumnIndexDefinition.requireVolume]);
            itemInfo.needMeasure = thisRowStrs[(int)ColumnIndexDefinition.needMeasure] == "Y";
            itemInfo.addWater2DiluteVolume = bool.Parse(thisRowStrs[(int)ColumnIndexDefinition.addWater2DiluteVolume]);
            itemInfo.orgVolumePerSlice = double.Parse(thisRowStrs[(int)ColumnIndexDefinition.orgVolumePerSlice]);
            itemInfo.needPipetting = thisRowStrs[(int)ColumnIndexDefinition.needPipetting] == "Y"; ;
            if(!itemInfo.needMeasure)
                itemInfo.orgConcentration = double.Parse(thisRowStrs[(int)ColumnIndexDefinition.orgConcentration]);
            itemInfo.addWater4MeasureVolume = double.Parse(thisRowStrs[(int)ColumnIndexDefinition.addWater4MeasureVolume]);
            itemInfo.addSample4MeasureVolume = double.Parse(thisRowStrs[(int)ColumnIndexDefinition.addSample4MeasureVolume]);
            itemInfo.needNormalization = bool.Parse(thisRowStrs[(int)ColumnIndexDefinition.needNormalization]);
            itemInfo.orgSampleTotalVolume = double.Parse(thisRowStrs[(int)ColumnIndexDefinition.orgSampleTotalVolume]);
            itemInfo.dstConcentration = double.Parse(thisRowStrs[(int)ColumnIndexDefinition.dstConcentration]);
            itemInfo.maxNormalizationVolume = double.Parse(thisRowStrs[(int)ColumnIndexDefinition.maxNormalizationVolume]);
          
            //itemInfo.judgeStandard = double.Parse(thisRowStrs[(int)ColumnIndexDefinition.judgeStandard]);

        }

        private DstLabwareType GetDstLabwareType(string s)
        {
            foreach (DstLabwareType labwareType in Enum.GetValues(typeof(DstLabwareType)))
            {
                if (s == labwareType.ToString())
                    return labwareType;
            }
            return DstLabwareType.Well96;
        }
    }
}
