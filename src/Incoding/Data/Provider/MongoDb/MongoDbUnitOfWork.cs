namespace Incoding.Data
{
    #region << Using >>

    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    #endregion

    [UsedImplicitly, ExcludeFromCodeCoverage]
    public class MongoDbUnitOfWork : UnitOfWorkBase<MongoDatabaseDisposable>
    {
        #region Constructors

        public MongoDbUnitOfWork(MongoDatabaseDisposable session, IsolationLevel level)
                : base(session)
        {
            repository = new MongoDbRepository(session);
        }

        #endregion

        protected override void InternalSubmit() { }

        protected override void InternalFlush() { }

        protected override void InternalCommit() { }

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