namespace Incoding.Data
{
    #region << Using >>

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    #endregion

    public interface IUnitOfWork : IDisposable            
    {
        void Flush();

        Task FlushAsync(CancellationToken ct = default);

        IRepository GetRepository();

        void Commit();

        Task CommitAsync(CancellationToken ct = default);
    }
}