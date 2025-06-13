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

    public class GetEntitiesQuery<T> : QueryBase<List<T>> where T : class, IEntity, new()
    {
        #region Override

        /// <inheritdoc/>
        protected override List<T> ExecuteResult()
        {
            return Repository.Query<T>().ToList();
        }

        /// <inheritdoc/>
        protected override Task<List<T>> ExecuteResultAsync(CancellationToken ct = default)
        {
            return Repository.Query<T>().ToListAsync(ct);
        }

        #endregion
    }
}