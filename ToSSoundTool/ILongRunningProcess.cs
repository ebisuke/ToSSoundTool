using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSSoundTool
{
    public interface ILongRunningProcess
    {
        event EventHandler<string> OnMessage;

        int Progress
        {
            get;
        }

        

        void Run();
        void Cancel();
    }
}
