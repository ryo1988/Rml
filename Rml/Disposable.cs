﻿using System;
using System.Reactive.Disposables;
using System.Threading;

namespace Rml
{
    /// <summary>
    /// 
    /// </summary>
    public class Disposable : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly CompositeDisposable Cd = new CompositeDisposable();

        private int _isDisposed;

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.Exchange(ref this._isDisposed, 1) == 1)
                return;
            this.Dispose(true);
            GC.SuppressFinalize((object) this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            Cd.Dispose();
        }
    }
}