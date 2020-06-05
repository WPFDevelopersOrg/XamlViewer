using System;

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
            _endDefer?.Invoke();
            _endDefer = null;

            GC.SuppressFinalize(this);
        }
    }
}
