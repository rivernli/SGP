using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SGP.DBUtility;
using System.Data;
using System.Data.SqlClient;


namespace BI.SGP.BLL.Models
{
    public class RFQDetail
    {

        public int ID
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public int RFQID
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Number
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerPartNumber
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Revision
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime RFQDateIn
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime RFQDateOut
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime QuoteDateIn
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime QuoteDateOut
        {
            get;
            set;
        }

        public DateTime PriceDateOut
        {
            get;
            set;
        }
        public string CostStopTime
        {
            get;
            set;

        }
        public string CostReStartTime
        {
            get;
            set;

        }
        public DateTime LastestRFQDateIn
        {
            get;
            set;

        }
        /// <summary>
        /// 
        /// </summary>
        public string Purpose
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string RFQStyle
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string PricingCommitLevel
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string TypeOfQuote
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string QTA
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string RequestDFM
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string RequestVVI
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string RequoteReason
        {
            get;
            set;
        }
        ///
        /// 
        public DateTime CustomerQuoteDate
        {
            get;
            set;
        }
        public string ExtNumber
        {
            get;
            set;
        }
        public string InternalRevisionNumber
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double DueDate
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string VIForm
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Initiator
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string PricingAnalyst
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string TechnicalQuoting
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string PrimaryContact
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string GAMBDM
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string OEM
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string CEM
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerContact
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string ProgramName
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string MarketSegment
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Application
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime MassProductionDate
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public int VolumePerMonth
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public int ExpectedLifeTime
        {
            get;
            set;
        }

        private double estQuoteSize = 0;
        /// <summary>
        /// 
        /// </summary>
        public double EstQuoteSize
        {
            get
            {
                if (estQuoteSize > 0)
                {

                    return estQuoteSize;
                }
                else
                {
                    double i = 0;

                    List<double> ps = new List<double>() { UnitPrice1, UnitPrice2, UnitPrice3, UnitPrice4, UnitPrice5, UnitPrice6, UnitPrice7, UnitPrice9, UnitPrice10 };
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
                    if (minprice > 0 && VolumePerMonth > 0)
                    {
                        i = (minprice * VolumePerMonth) / 1000;
                    }
                    i = Math.Round(i, 2);
                    return Math.Round(i, 0);
                }
            }
            set
            {
                estQuoteSize = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public double MinComputedSize
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Stage
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string FTPMail
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Comments
        {
            get;
            set;
        }
        public string ShipmentTerms
        {
            get;
            set;
        }
        public string Location
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string PayTerms
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Currency
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double ExchangRatePerUSD
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double SetupCharge
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double EtestCharge
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double ToolingCharge
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double ShipTermsAdder
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string LeadTime
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
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

                List<double> ps = new List<double>() { UnitPrice1, UnitPrice2, UnitPrice3, UnitPrice4, UnitPrice5, UnitPrice6, UnitPrice7, UnitPrice9, UnitPrice10 };
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

        /// <summary>
        /// 
        /// </summary>
        public string Notes
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Notes2
        {
            get;
            set;
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
        /// <summary>
        /// 
        /// </summary>
        public double TargetASP
        {
            get
            {

                double i = 0;
                if (TargetPrice > 0 && UnitPerWorkingPanel > 0 && PanelSizeWidth > 0 && PanelSizeLength > 0)
                {
                    i = 1 * TargetPrice * UnitPerWorkingPanel;
                    i = i / PanelSizeWidth / PanelSizeLength;

                    if (i > 0)
                    {
                        i = 144 * i;
                    }

                }
                return Math.Round(i, 4);

            }
            set { }

        }
        /// <summary>
        /// 
        /// </summary>
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
                List<double> ps = new List<double>() { UnitPrice1, UnitPrice2, UnitPrice3, UnitPrice4, UnitPrice5, UnitPrice6, UnitPrice7, UnitPrice9, UnitPrice10 };
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



                if (ExchangRatePerUSD > 0 && minprice > 0 && shiptermsadder > 0 && UnitPerWorkingPanel > 0 && PanelSizeWidth > 0 && PanelSizeLength > 0)
                {
                    i = ExchangRatePerUSD * minprice * UnitPerWorkingPanel;
                    i = i / PanelSizeLength / PanelSizeWidth / shiptermsadder;

                    if (i > 0)
                    {
                        i = 144 * i;
                    }
                }



                return Math.Round(i, 4);
            }
            set { }

        }


        public double TargetASPL
        {
            get
            {

                double i = 0;
                if (LayerCount > 0 && TargetPrice > 0 && UnitPerWorkingPanel > 0 && PanelSizeWidth > 0 && PanelSizeLength > 0)
                {
                    i = 1 * TargetPrice * UnitPerWorkingPanel;
                    i = i / PanelSizeWidth / PanelSizeLength/LayerCount;

                    if (i > 0)
                    {
                        i = 144 * i;
                    }

                }
                return Math.Round(i, 4);

            }
            set { }

        }
        public double MinASPL
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
                List<double> ps = new List<double>() { UnitPrice1, UnitPrice2, UnitPrice3, UnitPrice4, UnitPrice5, UnitPrice6, UnitPrice7, UnitPrice9, UnitPrice10 };
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



                if (LayerCount > 0 && ExchangRatePerUSD > 0 && minprice > 0 && shiptermsadder > 0 && UnitPerWorkingPanel > 0 && PanelSizeWidth > 0 && PanelSizeLength > 0)
                {
                    i = ExchangRatePerUSD * minprice * UnitPerWorkingPanel;
                    i = i / PanelSizeLength / PanelSizeWidth / shiptermsadder/LayerCount;

                    if (i > 0)
                    {
                        i = 144 * i;
                    }
                }



                return Math.Round(i, 4);
            }
            set { }
        }
        /// <summary>
        /// 
        /// </summary>
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
                return Math.Round(i, 4);



            }
            set { }
        }
        /// <summary>
        /// Min c/sqin, 每平方英寸的最小价格
        /// </summary>
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



                List<double> ps = new List<double>() { UnitPrice1, UnitPrice2, UnitPrice3, UnitPrice4, UnitPrice5, UnitPrice6, UnitPrice7, UnitPrice9, UnitPrice10 };
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
                return Math.Round(i * 100, 4);
            }
            set { }
        }
        /// <summary>
        /// 
        /// </summary>
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
                return Math.Round(i, 4);


            }
            set { }
        }
        /// <summary>
        /// 
        /// </summary>
        public double MinCLsqin
        {
            get
            {

                double i = 0;
                if (MinSqInch > 0 && LayerCount > 0)
                {
                    i = MinSqInch / LayerCount;

                }
                return Math.Round(i, 4);

            }
            set { }

        }
        /// <summary>
        /// 
        /// </summary>
        public double TargetPrice
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double TargetVSActucal
        {
            get
            {
                double i = 0;

                List<double> ps = new List<double>() { UnitPrice1, UnitPrice2, UnitPrice3, UnitPrice4, UnitPrice5, UnitPrice6, UnitPrice7, UnitPrice9, UnitPrice10 };
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
                return Math.Round(i, 4);

            }
            set { }
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price1Qty
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price2Qty
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price3Qty
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price4Qty
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price5Qty
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price6Qty
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price7Qty
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price8Qty
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price9Qty
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price10Qty
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double UnitPrice1
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double UnitPrice2
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double UnitPrice3
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double UnitPrice4
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double UnitPrice5
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double UnitPrice6
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double UnitPrice7
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double UnitPrice8
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double UnitPrice9
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double UnitPrice10
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price1Type
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price2Type
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price3Type
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price4Type
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Price5Type
        {
            get;
            set;
        }
        public string Price6Type
        {
            get;
            set;
        }
        public string Price7Type
        {
            get;
            set;
        }
        public string Price8Type
        {
            get;
            set;
        }
        public string Price9Type
        {
            get;
            set;
        }
        public string Price10Type
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Remark1
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Remark2
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Remark3
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Remark4
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Remark5
        {
            get;
            set;
        }
        public string Remark6
        {
            get;
            set;
        }
        public string Remark7
        {
            get;
            set;
        }
        public string Remark8
        {
            get;
            set;
        }
        public string Remark9
        {
            get;
            set;
        }
        public string Remark10
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Remarks
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string RFQDelayStatusUpdate
        {
            get;
            set;
        }
        public string CostingDelayStatus
        {
            get;
            set;

        }
        public string ViaStructure
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerStackUp
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double LayerCount
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string UnitOrArray
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double Holes
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double SmallestHole
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string PanelUtilization
        {
            get
            {
                double d = 0;
                if (UnitPerWorkingPanel > 0 && ArraySizeWidth > 0 && ArraySizeLength > 0 && PanelSizeWidth > 0 && PanelSizeLength > 0 && UnitPerArray > 0)
                {
                    double p = 100;
                    p = p * UnitPerWorkingPanel * ArraySizeWidth * ArraySizeLength;
                    p = p / PanelSizeLength / PanelSizeWidth / UnitPerArray;

                    if (string.IsNullOrEmpty(UnitType) == false && UnitType.ToLower() == "mm")
                    {
                        p = p / 25.4 / 25.4;
                    }
                    d = p;
                    return Math.Round(d, 2).ToString() + "%";
                }
                return Math.Round(d, 2).ToString() + "%";
            }
            set { }
        }
        /// <summary>
        /// 
        /// </summary>
        public string UnitType
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double UnitSizeWidth
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double UnitSizeLength
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string MaterialCategory
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string LaminateType
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double UnitPerArray
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double ArraySizeWidth
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double ArraySizeLength
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string USize
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string UQty
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double UnitPerWorkingPanel
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double PanelSizeWidth
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double PanelSizeLength
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string BlindSize
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string BlindQty
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double BoardThickness
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string LnO
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string LnI
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string BuriedSize
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string BuriedQty
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Finishing
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Finger
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Outline
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Imped
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Copper
        {
            get;
            set;
        }
        public string XOutAllowance
        {
            get;
            set;
        }

        public string GoldThickness
        {
            get;
            set;

        }
        public string GoldArea
        {
            get;
            set;
        }
        public string SolidViaPlating
        {
            get;
            set;

        }
        public string Class3
        {
            get;
            set;

        }
        /// <summary>
        /// 
        /// </summary>
        public string TechnicalRemarks
        {
            get;
            set;
        }
        public string HitRateStatus
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string HitRateStatusReason
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime ExpectedRFQCloseDate
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string PriceFeedback
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double CompetitiveWinPrice1
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double CompetitiveWinPrice2
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string HitRateStatusDetailed
        {
            get;
            set;
        }
        public string ProjectNumber
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Building
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double Yield
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double MaterialCost
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double VariableCost
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double FixedCost
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public double TotalCost
        {

            get
            {
                double i = 0;
                i = VariableCost + FixedCost;
                return Math.Round(i, 4); ;

            }
            set { }
        }
        /// <summary>
        /// 
        /// </summary>
        public double OP
        {
            get
            {
                double i = 0;
                if (UnitPrice1 > 0 && TotalCost > 0)
                {
                    i = (UnitPrice1 - TotalCost) / UnitPrice1;
                }
                return Math.Round(i * 100, 2); ;

            }
            set { }

        }
        public double OP1
        {
            get
            {
                double i = 0;
                if (UnitPrice1 > 0 && TotalCost > 0)
                {
                    i = (UnitPrice1 - TotalCost) / UnitPrice1;
                }
                return Math.Round(i * 100, 2);

            }
            set { }

        }
        public double OP2
        {
            get
            {
                double i = 0;
                if (UnitPrice2 > 0 && TotalCost > 0)
                {
                    i = (UnitPrice2 - TotalCost) / UnitPrice2;
                }
                return Math.Round(i * 100, 2);

            }
            set { }

        }
        public double OP3
        {
            get
            {
                double i = 0;
                if (UnitPrice3 > 0 && TotalCost > 0)
                {
                    i = (UnitPrice3 - TotalCost) / UnitPrice3;
                }
                return Math.Round(i * 100, 2);

            }
            set { }

        }
        public double OP4
        {
            get
            {
                double i = 0;
                if (UnitPrice4 > 0 && TotalCost > 0)
                {
                    i = (UnitPrice4 - TotalCost) / UnitPrice4;
                }
                return Math.Round(i * 100, 2);

            }
            set { }

        }
        public double OP5
        {
            get
            {
                double i = 0;
                if (UnitPrice5 > 0 && TotalCost > 0)
                {
                    i = (UnitPrice5 - TotalCost) / UnitPrice5;
                }
                return Math.Round(i * 100, 2);

            }
            set { }

        }
        public double OP6
        {
            get
            {
                double i = 0;
                if (UnitPrice6 > 0 && TotalCost > 0)
                {
                    i = (UnitPrice6 - TotalCost) / UnitPrice6;
                }
                return Math.Round(i * 100, 2);

            }
            set { }

        }
        public double OP7
        {
            get
            {
                double i = 0;
                if (UnitPrice7 > 0 && TotalCost > 0)
                {
                    i = (UnitPrice7 - TotalCost) / UnitPrice7;
                }
                return Math.Round(i * 100, 2);

            }
            set { }

        }
        public double OP8
        {
            get
            {
                double i = 0;
                if (UnitPrice8 > 0 && TotalCost > 0)
                {
                    i = (UnitPrice8 - TotalCost) / UnitPrice8;
                }
                return Math.Round(i * 100, 2);

            }
            set { }

        }
        public double OP9
        {
            get
            {
                double i = 0;
                if (UnitPrice9 > 0 && TotalCost > 0)
                {
                    i = (UnitPrice9 - TotalCost) / UnitPrice9;
                }
                return Math.Round(i * 100, 2);

            }
            set { }

        }
        public double OP10
        {
            get
            {
                double i = 0;
                if (UnitPrice10 > 0 && TotalCost > 0)
                {
                    i = (UnitPrice10 - TotalCost) / UnitPrice10;
                }
                return Math.Round(i * 100, 2);

            }
            set { }

        }
        /// <summary>
        /// 
        /// </summary>
        public double MP
        {
            get
            {
                double i = 0;
                if (UnitPrice1 > 0 && VariableCost > 0)
                {
                    i = (UnitPrice1 - VariableCost) / UnitPrice1;
                }
                return Math.Round(i * 100, 2);

            }

            set { }
        }
        public double MP1
        {
            get
            {
                double i = 0;
                if (UnitPrice1 > 0 && VariableCost > 0)
                {
                    i = (UnitPrice1 - VariableCost) / UnitPrice1;
                }
                return Math.Round(i * 100, 2);

            }

            set { }
        }
        public double MP2
        {
            get
            {
                double i = 0;
                if (UnitPrice2 > 0 && VariableCost > 0)
                {
                    i = (UnitPrice2 - VariableCost) / UnitPrice2;
                }
                return Math.Round(i * 100, 2);

            }

            set { }
        }
        public double MP3
        {
            get
            {
                double i = 0;
                if (UnitPrice3 > 0 && VariableCost > 0)
                {
                    i = (UnitPrice3 - VariableCost) / UnitPrice3;
                }
                return Math.Round(i * 100, 2);

            }

            set { }
        }
        public double MP4
        {
            get
            {
                double i = 0;
                if (UnitPrice4 > 0 && VariableCost > 0)
                {
                    i = (UnitPrice4 - VariableCost) / UnitPrice4;
                }
                return Math.Round(i * 100, 2);

            }

            set { }
        }
        public double MP5
        {
            get
            {
                double i = 0;
                if (UnitPrice5 > 0 && VariableCost > 0)
                {
                    i = (UnitPrice5 - VariableCost) / UnitPrice5;
                }
                return Math.Round(i * 100, 2);

            }

            set { }
        }
        public double MP6
        {
            get
            {
                double i = 0;
                if (UnitPrice6 > 0 && VariableCost > 0)
                {
                    i = (UnitPrice6 - VariableCost) / UnitPrice6;
                }
                return Math.Round(i * 100, 2);

            }

            set { }
        }
        public double MP7
        {
            get
            {
                double i = 0;
                if (UnitPrice7 > 0 && VariableCost > 0)
                {
                    i = (UnitPrice7 - VariableCost) / UnitPrice7;
                }
                return Math.Round(i * 100, 2);

            }

            set { }
        }
        public double MP8
        {
            get
            {
                double i = 0;
                if (UnitPrice8 > 0 && VariableCost > 0)
                {
                    i = (UnitPrice8 - VariableCost) / UnitPrice8;
                }
                return Math.Round(i * 100, 2);

            }

            set { }
        }
        public double MP9
        {
            get
            {
                double i = 0;
                if (UnitPrice9 > 0 && VariableCost > 0)
                {
                    i = (UnitPrice9 - VariableCost) / UnitPrice9;
                }
                return Math.Round(i * 100, 2);

            }

            set { }
        }
        public double MP10
        {
            get
            {
                double i = 0;
                if (UnitPrice10 > 0 && VariableCost > 0)
                {
                    i = (UnitPrice10 - VariableCost) / UnitPrice10;
                }
                return Math.Round(i * 100, 2);

            }

            set { }
        }
        /// <summary>
        /// 
        /// </summary>
        public string ManagementApproved
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string ApproverName
        {
            get;
            set;
        }

        public double VIPPO
        {
            get;
            set;
        }
        public double BackDrilling
        {
            get;
            set;
        }
        public string Coverlay
        {
            get;
            set;
        }
        public string DepthControlDrill
        {
            get;
            set;
        }
        public string EdgePlating
        {
            get;
            set;
        }
        public string Region
        {
            get;
            set;
        }
        public double OEMPCBSpend
        {
            get;
            set;
        }
        public string HMLVSpend
        {
            get;
            set;
        }
        public string VVIShipmentTerm
        {
            get;
            set;
        }
        public string VVICurrency
        {
            get;
            set;
        }
        public string VVILeadTime
        {
            get;
            set;
        }
        public double VVIMOV
        {
            get;
            set;
        }
        public string VVINRECharge
        {
            get;
            set;
        }
        public string VVIQuotationRemarks
        {
            get;
            set;
        }
        public string CapabilityCheck
        {
            get;
            set;
        }


    }
}