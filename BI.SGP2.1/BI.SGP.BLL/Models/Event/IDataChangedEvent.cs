using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI.SGP.BLL.Event
{
    public interface IDataChangedEvent
    {
        void DoBefore();
        void DoAfter();
    }
}
