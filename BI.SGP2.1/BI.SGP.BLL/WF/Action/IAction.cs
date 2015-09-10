using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI.SGP.BLL.WF.Action
{
    public interface IAction
    {
        void DoAction(WFActivity activity);
    }
}
