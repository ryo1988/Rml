using System;
using System.Reactive.Disposables;
using System.Threading;

namespace Rml
{
    public class Disposable : IDisposable
    {
        protected readonly CompositeDisposable Cd = new CompositeDisposable();

        private int _isDisposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this._isDisposed, 1) == 1)
                return;
            this.Dispose(true);
            GC.SuppressFinalize((object) this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Cd.Dispose();
        }
    }
}