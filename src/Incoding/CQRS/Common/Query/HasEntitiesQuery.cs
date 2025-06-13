namespace Incoding.CQRS
{
    #region << Using >>

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Incoding.Block;
    using Incoding.Data;
    using NHibernate.Linq;

    #endregion

    public class HasEntitiesQuery<TEntity> : QueryBase<IncBoolResponse> where TEntity : class, IEntity, new()
    {
        /// <inheritdoc/>
        protected override IncBoolResponse ExecuteResult()
        {
            return Repository.Query<TEntity>().Any();
        }

        /// <inheritdoc/>
        protected override async Task<IncBoolResponse> ExecuteResultAsync(CancellationToken ct = default)
        {
            return await Repository.Query<TEntity>().AnyAsync(ct);
        }
    }
}