namespace Incoding.CQRS
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Incoding.Block;

    public abstract class QueryBase<TResult> : MessageBase
    {
        #region Override

        protected override void Execute()
        {
            Result = ExecuteResult();
        }

        protected override async Task ExecuteAsync(CancellationToken ct = default)
        {
            Result = await ExecuteResultAsync(ct);
        }

        #endregion

        protected abstract TResult ExecuteResult();

        // No longer abstract — this is now the default async fallback
        protected virtual Task<TResult> ExecuteResultAsync(CancellationToken ct = default)
        {
            return Task.FromResult(ExecuteResult());
        }
    }
}