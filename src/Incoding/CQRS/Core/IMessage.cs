namespace Incoding.CQRS
{
    #region << Using >>

    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Incoding.Data;
    using Newtonsoft.Json;

    #endregion

    public interface IMessage 
    {
        object Result { get; }
        
        MessageExecuteSetting Setting { get; set; }

        void OnExecute(IDispatcher current, Lazy<IUnitOfWork> unitOfWork);

        Task OnExecuteAsync(IDispatcher current, Lazy<IUnitOfWork> unitOfWork, CancellationToken ct = default);
    }
}