using System;
using System.Threading;

namespace Samba.Infrastructure.ExceptionReporter
{
    /// <summary>
    /// Base class for all classes wanting to implement <see cref="IDisposable"/>.
    /// </summary>
    /// <remarks>
    /// Base on an article by Davy Brion 
    /// <see href="http://davybrion.com/blog/2008/06/disposing-of-the-idisposable-implementation/"/>.
    /// </remarks>
    public abstract class Disposable : IDisposable
    {
        private int disposed;

        protected Disposable()
        {
            disposed = 0;
        }

        public bool IsDisposed
        {
            get { return disposed == 1; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 0)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                }
                DisposeUnmanagedResources();
            }
        }

    	protected virtual void DisposeManagedResources() {}
        protected virtual void DisposeUnmanagedResources() {}

        ~Disposable()
        {
            Dispose(false);
        }
    }
}