namespace Incoding.Data
{
    #region << Using >>

    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Caching;

    #endregion

    public abstract class UnitOfWorkBase<TSession> : IUnitOfWork
            where TSession : class, IDisposable
    {
        #region Constructors

        protected UnitOfWorkBase(TSession session)
        {
            this.session = session;
        }

        #endregion

        #region Disposable

        public void Dispose()
        {
            if (!disposed)
            {
                InternalSubmit();
                session.Dispose();
            }

            disposed = true;
        }

        #endregion

        protected abstract void InternalSubmit();

        protected abstract void InternalFlush();

        protected abstract Task InternalFlushAsync(CancellationToken ct = default);

        protected abstract void InternalCommit();

        protected abstract Task InternalCommitAsync(CancellationToken ct = default);

        #region Fields

        protected readonly TSession session;

        protected IRepository repository;

        bool disposed;

        #endregion

        #region IUnitOfWork Members

        public IRepository GetRepository()
        {
            return repository;
        }

        public void Commit()
        {
            InternalCommit();
        }

        public async Task CommitAsync(CancellationToken ct = default)
        {
            await InternalCommitAsync(ct);
        }

        public void Flush()
        {
            if (!disposed)
                InternalFlush();
        }

        public async Task FlushAsync(CancellationToken ct = default)
        {
            if (!disposed)
                await InternalFlushAsync(ct);
        }

        #endregion
    }
}