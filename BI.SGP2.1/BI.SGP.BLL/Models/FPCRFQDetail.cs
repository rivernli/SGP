using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI.SGP.BLL.Models
{
   public class FPCRFQDetail
    {
       
        /// <summary>
        /// 
        /// </summary>
        /// 
        public int VolumePerMonth
        {
            get;
            set;
        }
        public double DueDate
        {
            get;
            set;
        }
        public double ExpectedLifeTime
        {
            get;
            set;
        }
        public double EstQuoteSize
        {
            get;
            set;
        }
        public double MinComputedSize
        {
            get;
            set;
        }
        public string Currency
        {
            get;
            set;
        }
        public double ExchangRatePerUSD
        {
            get;
            set;
        }
        public double SetupCharge
        {
            get;
            set;
        }
        public double EtestCharge
        {
            get;
            set;
        }
        public double ToolingCharge
        {
            get;
            set;
        }
        public double ShipTermsAdder
        {
            get;
            set;
        }
        public double MOV
        {
            get;
            set;
        }
        public double MOQ
        {
            get
            {

                double i = 0;

                List<double> ps = new List<double>() { PCBFPCPrice1, PCBFPCPrice2, PCBFPCPrice3, PCBFPCPrice4, PCBFPCPrice5 };
                List<double> psexistsval = new List<double>();

                foreach (double d in ps)
                {
                    if (d > 0)
                    {
                        psexistsval.Add(d);
                    }
                }
                double minprice = 0;
                if (psexistsval.Count > 0)
                {
                    minprice = psexistsval.Min();
                }
                if (minprice > 0 && MOV > 0)
                {
                    i = MOV / minprice;
                }
                i = Math.Round(i, 1) + 0.5;
                return Math.Round(i, 0);
            }
            set { }
        }
        public double TargetPrice1
        {
            get
            {
                double i = 0;
                i = TargetPrice;
                return i;

            }
            set { }

        }
        public double TargetASP
        {
            get
            {

                double i = 0;
                if (TargetPrice > 0 && UnitPerArray > 0 &&ArrayPerWorkingPanel>0 && PanelSizeWidth > 0 && PanelSizeLength > 0)
                {
                    i = 1 * TargetPrice * UnitPerArray*ArrayPerWorkingPanel;
                    i = i / PanelSizeWidth / PanelSizeLength;

                    if (i > 0)
                    {
                        i = 144 * i;
                    }

                }
                return Math.Round(i, 6);

            }
            set { }

        }
        public double MinASP
        {
            get
            {
                double i = 0;

                double shiptermsadder = ShipTermsAdder;
                if (shiptermsadder == 0)
                {
                    shiptermsadder = 1;
                }
                else
                {
                    shiptermsadder = shiptermsadder / 100;
                }

                if (ExchangRatePerUSD == 0)
                {
                    ExchangRatePerUSD = 1;
                }
                List<double> ps = new List<double>() { PCBFPCPrice1, PCBFPCPrice2, PCBFPCPrice3, PCBFPCPrice4, PCBFPCPrice5 };
                List<double> psexistsval = new List<double>();

                foreach (double d in ps)
                {
                    if (d > 0)
                    {
                        psexistsval.Add(d);
                    }
                }
                double minprice = 0;
                if (psexistsval.Count > 0)
                {
                    minprice = psexistsval.Min();
                }



                if (ExchangRatePerUSD > 0 && minprice > 0 && shiptermsadder > 0 && UnitPerArray>0 && ArrayPerWorkingPanel>0 && PanelSizeWidth > 0 && PanelSizeLength > 0)
                {
                    i = ExchangRatePerUSD * minprice * UnitPerArray*ArrayPerWorkingPanel;
                    i = i / PanelSizeLength / PanelSizeWidth / shiptermsadder;

                    if (i > 0)
                    {
                        i = 144 * i;
                    }
                }



                return Math.Round(i, 6);
            }
            set { }

        }
        public double TargetSqIn
        {
            get
            {
                double i = 0;
                if (TargetPrice > 0 && UnitPerArray > 0 && ArraySizeLength > 0 && ArraySizeWidth > 0)
                {
                    i = 100 * TargetPrice * UnitPerArray;
                    i = i / ArraySizeWidth / ArraySizeLength;
                    if (string.IsNullOrEmpty(UnitType) == false && UnitType.ToLower() == "mm")
                    {
                        i = i * 25.4 * 25.4;

                    }

                }
                return Math.Round(i, 6);



            }
            set { }
        }
        public double MinSqInch
        {
            get
            {

                double i = 0;
                double shiptermsadder = ShipTermsAdder;

                if (shiptermsadder > 0)
                {
                    shiptermsadder = shiptermsadder / 100;
                }
                else
                {
                    shiptermsadder = 1d;
                }


                if (ExchangRatePerUSD == 0)
                {
                    ExchangRatePerUSD = 1;
                }



                List<double> ps = new List<double>() { PCBFPCPrice1, PCBFPCPrice2, PCBFPCPrice3, PCBFPCPrice4, PCBFPCPrice5 };
                List<double> psexistsval = new List<double>();

                foreach (double d in ps)
                {
                    if (d > 0)
                    {
                        psexistsval.Add(d);
                    }
                }
                double minprice = 0;
                if (psexistsval.Count > 0)
                {
                    minprice = psexistsval.Min();
                }


                if (ExchangRatePerUSD > 0 && minprice > 0 && shiptermsadder > 0 && UnitPerArray > 0 && ArraySizeWidth > 0 && ArraySizeLength > 0)
                {
                    i = ExchangRatePerUSD * minprice * UnitPerArray;
                    i = i / ArraySizeLength / ArraySizeWidth / shiptermsadder;
                    if (string.IsNullOrEmpty(UnitType) == false && UnitType.ToLower() == "mm")
                    {
                        i = i * 25.4 * 25.4;
                    }

                }
                return Math.Round(i * 100, 6);
            }
            set { }
        }
        public double TargetCLsqin
        {
            get
            {

                double i = 0;

                if (UnitPerArray > 0 && TargetPrice > 0 && LayerCount > 0 && ArraySizeLength > 0 && ArraySizeWidth > 0)
                {

                    i = 100 * UnitPerArray * TargetPrice / (LayerCount * ArraySizeLength * ArraySizeWidth);
                    if (string.IsNullOrEmpty(UnitType) == false && UnitType.ToLower() == "mm")
                    {
                        i = i * 25.4 * 25.4;

                    }
                }
                return Math.Round(i, 6);


            }
            set { }
        }
        public double MinCLsqin
        {
            get
            {

                double i = 0;
                if (MinSqInch > 0 && LayerCount > 0)
                {
                    i = MinSqInch / LayerCount;

                }
                return Math.Round(i, 6);

            }
            set { }

        }
        public double TargetPrice
        {
            get;
            set;
        }
        public double TargetVSActucal
        {
            get
            {
                double i = 0;

                List<double> ps = new List<double>() { PCBFPCPrice1, PCBFPCPrice2, PCBFPCPrice3, PCBFPCPrice4, PCBFPCPrice5 };
                List<double> psexistsval = new List<double>();

                foreach (double d in ps)
                {
                    if (d > 0)
                    {
                        psexistsval.Add(d);
                    }
                }
                double minprice = 0;
                if (psexistsval.Count > 0)
                {
                    minprice = psexistsval.Min();
                }


                if (minprice > 0 && TargetPrice > 0)
                {
                    i = (minprice - TargetPrice) / TargetPrice;
                }
                return Math.Round(i, 6);

            }
            set { }
        }
        public double AssemblyCost
        {
            get;
            set;
        }
        public double PCBFPCPrice1
        {
            get;
            set;
        }
        public double PCBFPCPrice2
        {
            get;
            set;
        }
        public double PCBFPCPrice3
        {
            get;
            set;
        }
        public double PCBFPCPrice4
        {
            get;
            set;
        }
        public double PCBFPCPrice5
        {
            get;
            set;
        }
        public double SMTBOMCost
        {
            get;
            set;
        }
        public double BOMCost1
        {
            get
            {
                double i = 0;
                i = SMTBOMCost;
                return i;

            }
            set { }
        }
        public double BOMCost2
        {
            get
            {
                double i = 0;
                i = SMTBOMCost;
                return i;

            }
            set { }
        }
        public double BOMCost3
        {
            get
            {
                double i = 0;
                i = SMTBOMCost;
                return i;

            }
            set { }
        }
        public double BOMCost4
        {
            get
            {
                double i = 0;
                i = SMTBOMCost;
                return i;

            }
            set { }
        }
        public double BOMCost5
        {
            get
            {
                double i = 0;
                i = SMTBOMCost;
                return i;

            }
            set { }
        }
        public double BOMMarkup1
        {
            get;
            set;
        }
        public double BOMMarkup2
        {
            get;
            set;
        }
        public double BOMMarkup3
        {
            get;
            set;
        }
        public double BOMMarkup4
        {
            get;
            set;
        }
        public double BOMMarkup5
        {
            get;
            set;
        }
        public double BOMPrice1
        {
            
            get
            {
                double i = 0;
                if (SMTBOMCost > 0 && BOMMarkup1 > 0)
                {
                    i = SMTBOMCost * BOMMarkup1;
                }
                return i;
            }
            set { }
        }
        public double BOMPrice2
        {
            get
            {
                double i = 0;
                if (SMTBOMCost > 0 && BOMMarkup2 > 0)
                {
                    i = SMTBOMCost * BOMMarkup2;
                }
                return i;
            }
            set { }
        }
        public double BOMPrice3
        {
            get
            {
                double i = 0;
                if (SMTBOMCost > 0 && BOMMarkup3 > 0)
                {
                    i = SMTBOMCost * BOMMarkup3;
                }
                return i;
            }
            set { }

        }
        public double BOMPrice4
        {
            get
            {
                double i = 0;
                if (SMTBOMCost > 0 && BOMMarkup4 > 0)
                {
                    i = SMTBOMCost * BOMMarkup4;
                }
                return i;
            }
            set { }
        }
        public double BOMPrice5
        {
            get
            {
                double i = 0;
                if (SMTBOMCost > 0 && BOMMarkup5 > 0)
                {
                    i = SMTBOMCost * BOMMarkup5;
                }
                return i;
            }
            set { }
        }
        public double AssemblyCost1
        {
            get
            {
                double i = 0;
                i = AssemblyCost;
                return i;

            }
            set { }
        }
        public double AssemblyCost2
        {
            get
            {
                double i = 0;
                i = AssemblyCost;
                return i;

            }
            set { }
        }
        public double AssemblyCost3
        {
            get
            {
                double i = 0;
                i = AssemblyCost;
                return i;

            }
            set { }

        }
        public double AssemblyCost4
        {
            get
            {
                double i = 0;
                i = AssemblyCost;
                return i;

            }
            set { }
        }
        public double AssemblyCost5
        {
            get
            {
                double i = 0;
                i = AssemblyCost;
                return i;

            }
            set { }
        }
        public double AssemblyMarkup1
        {
            get;
            set;
        }
        public double AssemblyMarkup2
        {
            get;
            set;
        }
        public double AssemblyMarkup3
        {
            get;
            set;
        }
        public double AssemblyMarkup4
        {
            get;
            set;
        }
        public double AssemblyMarkup5
        {
            get;
            set;
        }
        public double AssemblyPrice1
        {
            get
            {
                double i = 0;
                if (AssemblyCost > 0 && AssemblyMarkup1 > 0)
                {
                    i = AssemblyCost * AssemblyMarkup1;
                    
                }
                return Math.Round(i, 6);
            }
            set { }
        }
        public double AssemblyPrice2
        {
            get
            {
                double i = 0;
                if (AssemblyCost > 0 && AssemblyMarkup2 > 0)
                {
                    i = AssemblyCost * AssemblyMarkup2;

                }
                return Math.Round(i, 6);
            }
            set { }
        }
        public double AssemblyPrice3
        {
            get
            {
                double i = 0;
                if (AssemblyCost > 0 && AssemblyMarkup3 > 0)
                {
                    i = AssemblyCost * AssemblyMarkup3;

                }
                return Math.Round(i, 6);
            }
            set { }
        }
        public double AssemblyPrice4
        {
            get
            {
                double i = 0;
                if (AssemblyCost > 0 && AssemblyMarkup4 > 0)
                {
                    i = AssemblyCost * AssemblyMarkup4;

                }
                return Math.Round(i, 6);
            }
            set { }
        }
        public double AssemblyPrice5
        {
            get
            {
                double i = 0;
                if (AssemblyCost > 0 && AssemblyMarkup5 > 0)
                {
                    i = AssemblyCost * AssemblyMarkup5;

                }
                return Math.Round(i, 6);
            }
            set { }
        }
        public double TotalPrice1
        {
            get
            {
                double i = 0;
                i = PCBFPCPrice1 + BOMPrice1 + AssemblyPrice1;
                return Math.Round(i,6);
            }
            set { }
        }
        public double TotalPrice2
        {
            get
            {
                double i = 0;
                i = PCBFPCPrice2 + BOMPrice2 + AssemblyPrice2;
                return Math.Round(i, 6);
            }
            set { }
        }
        public double TotalPrice3
        {
            get
            {
                double i = 0;
                i = PCBFPCPrice3 + BOMPrice3 + AssemblyPrice3;
                return Math.Round(i, 6);
            }
            set { }
        }
        public double TotalPrice4
        {
            get
            {
                double i = 0;
                i = PCBFPCPrice4 + BOMPrice4 + AssemblyPrice4;
                return Math.Round(i, 6);
            }
            set { }
        }
        public double TotalPrice5
        {
            get
            {
                double i = 0;
                i = PCBFPCPrice5 + BOMPrice5 + AssemblyPrice5;
                return Math.Round(i, 6);
            }
            set { }
        }


        public string PriceType1
        {
            get;
            set;
        }
        public string PriceType2
        {
            get;
            set;
        }
        public string PriceType3
        {
            get;
            set;
        }
        public string PriceType4
        {
            get;
            set;
        }
        public string PriceType5
        {
            get;
            set;
        }
        public double LayerCount
        {
            get;
            set;
        }
        public string UnitOrArray
        {
            get;
            set;
        }
        public double Holes
        {
            get;
            set;
        }
        public double UnitArea
        {
            get;
            set;
        }
        public double SmallestHole
        {
            get;
            set;
        }
        public double ArrayPerWorkingPanel
        {
            get;
            set;
        }
        public string PanelUtilization
        {
            get
            {
                double d = 0;
                if (UnitArea > 0 && UnitPerArray > 0 && ArrayPerWorkingPanel > 0 && PanelSizeWidth > 0 && PanelSizeLength > 0)
                {
                    double p = 100;
                    p = p * UnitArea * UnitPerArray * ArrayPerWorkingPanel;
                    p = p / PanelSizeLength / PanelSizeWidth ;

                    if (string.IsNullOrEmpty(UnitType) == false && UnitType.ToLower() == "mm")
                    {
                        p = p / 25.4 / 25.4;
                    }
                    d = p;
                    return Math.Round(d, 6).ToString() + "%";
                }
                return Math.Round(d, 6).ToString() + "%";
            }
            set { }
        }
        public string UnitType
        {
            get;
            set;
        }
        public double UnitSizeWidth
        {
            get;
            set;
        }
        public double UnitSizeLength
        {
            get;
            set;
        }
        public double UnitPerArray
        {
            get;
            set;
        }
        public double ArraySizeWidth
        {
            get;
            set;
        }
        public double ArraySizeLength
        {
            get;
            set;
        }
        public double UnitPerWorkingPanel
        {
            get 
            {
                double i = 0;
                i = ArrayPerWorkingPanel * UnitPerArray;
                return i;
            }
            set { }
        }
        public double PanelSizeWidth
        {
            get;
            set;
        }
        public double PanelSizeLength
        {
            get;
            set;
        }
        public double BoardThickness
        {
            get;
            set;
        }
        public double CompetitiveWinPrice1
        {
            get;
            set;
        }
        public double CompetitiveWinPrice2
        {
            get;
            set;
        }
        public double Yield
        {
            get;
            set;
        }
        public double MaterialCost
        {
            get;
            set;
        }
        public double VariableCost
        {
            get;
            set;
        }
        public double FixedCost
        {
            get;
            set;
        }
        public double TotalCost
        {

            get
            {
                double i = 0;
                i = VariableCost + FixedCost;
                return Math.Round(i, 6); ;

            }
            set { }
        }
        public double OP
        {
            get
            {
                double i = 0;
                if (PCBFPCPrice1 > 0 && TotalCost > 0)
                {
                    i = (PCBFPCPrice1 - TotalCost) / PCBFPCPrice1;
                }
                return Math.Round(i * 100, 6); ;

            }
            set { }

        }
        public double OP1
        {
            get
            {
                double i = 0;
                if (PCBFPCPrice1 > 0 && TotalCost > 0)
                {
                    i = (PCBFPCPrice1 - TotalCost) / PCBFPCPrice1;
                }
                return Math.Round(i * 100, 6);

            }
            set { }

        }
        public double OP2
        {
            get
            {
                double i = 0;
                if (PCBFPCPrice2 > 0 && TotalCost > 0)
                {
                    i = (PCBFPCPrice2 - TotalCost) / PCBFPCPrice2;
                }
                return Math.Round(i * 100, 6);

            }
            set { }

        }
        public double OP3
        {
            get
            {
                double i = 0;
                if (PCBFPCPrice3 > 0 && TotalCost > 0)
                {
                    i = (PCBFPCPrice3 - TotalCost) / PCBFPCPrice3;
                }
                return Math.Round(i * 100, 6);

            }
            set { }

        }
        public double OP4
        {
            get
            {
                double i = 0;
                if (PCBFPCPrice4 > 0 && TotalCost > 0)
                {
                    i = (PCBFPCPrice4 - TotalCost) / PCBFPCPrice4;
                }
                return Math.Round(i * 100, 6);

            }
            set { }

        }
        public double OP5
        {
            get
            {
                double i = 0;
                if (PCBFPCPrice5 > 0 && TotalCost > 0)
                {
                    i = (PCBFPCPrice5 - TotalCost) / PCBFPCPrice5;
                }
                return Math.Round(i * 100, 6);

            }
            set { }

        }
        public double MP
        {
            get
            {
                double i = 0;
                if (PCBFPCPrice1 > 0 && VariableCost > 0)
                {
                    i = (PCBFPCPrice1 - VariableCost) / PCBFPCPrice1;
                }
                return Math.Round(i * 100, 6);

            }

            set { }
        }
        public double MP1
        {
            get
            {
                double i = 0;
                if (PCBFPCPrice1 > 0 && VariableCost > 0)
                {
                    i = (PCBFPCPrice1 - VariableCost) / PCBFPCPrice1;
                }
                return Math.Round(i * 100, 6);

            }

            set { }
        }
        public double MP2
        {
            get
            {
                double i = 0;
                if (PCBFPCPrice2 > 0 && VariableCost > 0)
                {
                    i = (PCBFPCPrice2 - VariableCost) / PCBFPCPrice2;
                }
                return Math.Round(i * 100, 6);

            }

            set { }
        }
        public double MP3
        {
            get
            {
                double i = 0;
                if (PCBFPCPrice3 > 0 && VariableCost > 0)
                {
                    i = (PCBFPCPrice3 - VariableCost) / PCBFPCPrice3;
                }
                return Math.Round(i * 100, 6);

            }

            set { }
        }
        public double MP4
        {
            get
            {
                double i = 0;
                if (PCBFPCPrice4 > 0 && VariableCost > 0)
                {
                    i = (PCBFPCPrice4 - VariableCost) / PCBFPCPrice4;
                }
                return Math.Round(i * 100, 6);

            }

            set { }
        }
        public double MP5
        {
            get
            {
                double i = 0;
                if (PCBFPCPrice5 > 0 && VariableCost > 0)
                {
                    i = (PCBFPCPrice5 - VariableCost) / PCBFPCPrice5;
                }
                return Math.Round(i * 100, 6);

            }

            set { }
        }
        public double Total_1
        {
            get
            {
                double i = 0;
                i = OutlineTool1_1 + OutlineTool2_1 + TopCoverlayTool_1 + BottomCoverlayTool_1 + ETestCADArtworkNRE_1;
                return Math.Round(i, 6);
            }
            set
            {
            }
        }
        public double Total_2
        {
            get
            {
                double i = 0;
                i = OutlineTool1_2 + OutlineTool2_2 + TopCoverlayTool_2 + BottomCoverlayTool_2 + ETestCADArtworkNRE_2;
                return Math.Round(i, 6);
 
            }
            set
            {
            }
        }
        public double Total_3
        {
            get
            {
                double i = 0;
                i = OutlineTool1_3 + OutlineTool2_3 + TopCoverlayTool_3 + BottomCoverlayTool_3 + ETestCADArtworkNRE_3;
                return Math.Round(i, 6);

            }
            set
            {
            }
        }
        public double Total_4
        {
            get
            {
                double i = 0;
                i = OutlineTool1_4 + OutlineTool2_4 + TopCoverlayTool_4 + BottomCoverlayTool_4 + ETestCADArtworkNRE_4;
                return Math.Round(i, 6);

            }
            set{}
        }
        public double OutlineTool1_1
        {
            get;
            set;
        }
        public double OutlineTool1_2
        {
            get;
            set;
        }
        public double OutlineTool1_3
        {
            get;
            set;
        }
        public double OutlineTool1_4
        {
            get;
            set;
        }
        public double OutlineTool2_1
        {
            get;
            set;
        }
        public double OutlineTool2_2
        {
            get;
            set;
        }
        public double OutlineTool2_3
        {
            get;
            set;
        }
        public double OutlineTool2_4
        {
            get;
            set;
        }
        public double TopCoverlayTool_1
        {
            get;
            set;
        }
        public double TopCoverlayTool_2
        {
            get;
            set;
        }
        public double TopCoverlayTool_3
        {
            get;
            set;
        }
        public double TopCoverlayTool_4
        {
            get;
            set;
        }
        public double BottomCoverlayTool_1
        {
            get;
            set;
        }
        public double BottomCoverlayTool_2
        {
            get;
            set;
        }
        public double BottomCoverlayTool_3
        {
            get;
            set;
        }
        public double BottomCoverlayTool_4
        {
            get;
            set;
        }
        public double ETestCADArtworkNRE_1
        {
            get;
            set;
        }
        public double ETestCADArtworkNRE_2
        {
            get;
            set;
        }
        public double ETestCADArtworkNRE_3
        {
            get;
            set;
        }
        public double ETestCADArtworkNRE_4
        {
            get;
            set;
        }
        
    }
}
