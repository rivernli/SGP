using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BI.SGP.BLL.CostEngine.DataConverter;

namespace BI.SGP.BLL.CostEngine.ProjectCost
{
    public class CoinCost : CostBase
    {
        protected override string Formula
        {
            get
            {
                return "{LayupQtyPanel}*{ManualPrice}*{UnitPerWorkingPanel}";
            }
        }

        public CoinCost(object data, InputDataType dataType)
            : base(data, dataType)
        {

        }
    }
}
