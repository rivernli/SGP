using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BI.SGP.BLL.CostEngine.DataConverter;

namespace BI.SGP.BLL.CostEngine.ProjectCost
{
    public class CostItemRateCost : CostBase
    {
        protected override string Formula
        {
            get
            {
                if (ComputeByCycleTime)
                {
                    return "{TotalCycleTime}*{CostRate}";
                }
                else
                {
                    return "{PanelArea}*{CostRate}";
                }
            }
        }

        public CostItemRateCost(object data, InputDataType dataType)
            : base(data, dataType)
        {

        }
    }
}