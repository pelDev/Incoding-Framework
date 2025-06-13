#region copyright

// @incoding 2011

#endregion

namespace Incoding.CQRS
{
    using System.Threading;
    using System.Threading.Tasks;
    /// <summary>
    ///     Interface Dispatcher
    /// </summary>
    public interface IDispatcher
    {
        void Push(CommandComposite composite);

        Task PushAsync(CommandComposite composite, CancellationToken ct = default);

        TResult Query<TResult>(QueryBase<TResult> message, MessageExecuteSetting executeSetting = null);

        Task<TResult> QueryAsync<TResult>(QueryBase<TResult> message, MessageExecuteSetting executeSetting = null, CancellationToken ct = default);
    }
}