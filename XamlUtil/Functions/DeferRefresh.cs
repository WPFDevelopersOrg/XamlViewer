using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace XamlUtil.Functions
{  
    public class DeferRefresh : IDisposable
    {
        private Action _endDefer;

        public DeferRefresh(Action endDefer)
        {
            _endDefer = endDefer;
        }

        public void Dispose()
        {
            if (_endDefer != null)
            {
                _endDefer();
                _endDefer = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
