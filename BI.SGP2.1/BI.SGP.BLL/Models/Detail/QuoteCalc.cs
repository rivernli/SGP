using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI.SGP.BLL.Models.Detail
{
    public class QuoteCalc
    {
        public List<double> UnitPrice { get; set; }
        public List<double> OP { get; set; }
        public List<double> MP { get; set; }

        public double FixedCost { get; set; }
        public double MaterialCost { get; set; }
        public double VariableCost { get; set; }
        public double UnitPerWorkingPanel { get; set; }
        public double ArraySizeWidth { get; set; }
        public double ArraySizeLength { get; set; }
        public double PanelSizeWidth { get; set; }
        public double PanelSizeLength { get; set; }
        public double TargetPrice { get; set; }
        public double ExchangRatePerUSD { get; set; }
        public double ShipTermsAdder { get; set; }
        public double MinSqInch { get; set; }
        public double LayerCount { get; set; }
        public double UnitPerArray { get; set; }
        public double UnitType { get; set; }
        public double TotalCost { get; set; }
    }
}
