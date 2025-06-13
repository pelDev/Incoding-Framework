namespace Incoding.Data
{
    #region << Using >>

    using System.Data;
    using System.Data.Entity;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    #endregion

    [ExcludeFromCodeCoverage]
    public class EntityFrameworkUnitOfWork : UnitOfWorkBase<DbContext>
    {
        #region Fields

        readonly DbContextTransaction transaction;

        bool isWasCommit;

        #endregion

        #region Constructors

        public EntityFrameworkUnitOfWork(DbContext session, IsolationLevel level,bool isFlush)
                : base(session)
        {
            transaction = session.Database.BeginTransaction(level);
            if (!isFlush)
                session.Configuration.AutoDetectChangesEnabled = false;
            repository = new EntityFrameworkRepository(session);
        }

        #endregion

        protected override void InternalFlush()
        {
            session.SaveChanges();
        }

        protected override void InternalCommit()
        {
            transaction.Commit();
            isWasCommit = true;
        }

        protected override void InternalSubmit()
        {
            if (!isWasCommit)
                transaction.Rollback();

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