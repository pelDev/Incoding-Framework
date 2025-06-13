namespace Incoding.Data
{
    using System.Threading;
    using System.Threading.Tasks;
    #region << Using >>

    using System.Transactions;
    using Raven.Client;
    using IsolationLevel = System.Data.IsolationLevel;

    #endregion

    public class RavenDbUnitOfWork : UnitOfWorkBase<IDocumentSession>
    {
        #region Fields

        readonly TransactionScope transaction;

        #endregion

        #region Constructors

        public RavenDbUnitOfWork(IDocumentSession session,IsolationLevel level)
                : base(session)
        {
            transaction = new TransactionScope(TransactionScopeOption.RequiresNew);
            repository = new RavenDbRepository(session);
        }

        #endregion

        protected override void InternalFlush()
        {
            session.SaveChanges();
        }

        protected override void InternalCommit()
        {
            transaction.Complete();
        }

        protected override void InternalSubmit()
        {
            transaction.Dispose();
        }

        protected override Task InternalFlushAsync(CancellationToken ct = default)
        {
            throw new System.NotImplementedException();
        }

        protected override Task InternalCommitAsync(CancellationToken ct = default)
        {
            throw new System.NotImplementedException();
        }
    }
}