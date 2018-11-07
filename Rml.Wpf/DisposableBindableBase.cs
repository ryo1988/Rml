using System;
using System.Reactive.Disposables;
using System.Threading;
using Prism.Mvvm;

namespace Rml.Wpf
{
    public class DisposableBindableBase : BindableBase, IDisposable
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