namespace Incoding.CQRS
{
    #region << Using >>

    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Incoding.Maybe;

    #endregion

    public static class DispatcherExtensions
    {
        #region Factory constructors

        public static void Push(this IDispatcher dispatcher, Action<CommandComposite> configuration)
        {
            var composite = new CommandComposite();
            configuration(composite);
            dispatcher.Push(composite);
        }

        public static async Task PushAsync(this IDispatcher dispatcher, Action<CommandComposite> configuration, CancellationToken ct = default)
        {
            var composite = new CommandComposite();
            configuration(composite);
            await dispatcher.PushAsync(composite);
        }

        public static void Push(this IDispatcher dispatcher, CommandBase message, MessageExecuteSetting executeSetting = null)
        {
            dispatcher.Push(composite => composite.Quote(message, executeSetting));
        }

        public static async Task PushAsync(this IDispatcher dispatcher, CommandBase message, MessageExecuteSetting executeSetting = null, CancellationToken ct = default)
        {
            await dispatcher.PushAsync(composite => composite.Quote(message, executeSetting), ct);
        }

        public static void Push(this IDispatcher dispatcher, CommandBase message, Action<MessageExecuteSetting> configurationSetting)
        {
            var setting = new MessageExecuteSetting();
            configurationSetting.Do(action => action(setting));
            dispatcher.Push(message, setting);
        }

        public static async Task PushAsync(this IDispatcher dispatcher, CommandBase message, Action<MessageExecuteSetting> configurationSetting, CancellationToken ct = default)
        {
            var setting = new MessageExecuteSetting();
            configurationSetting.Do(action => action(setting));
            await dispatcher.PushAsync(message, setting, ct);
        }

        public static TResult Query<TResult>(this IDispatcher dispatcher, QueryBase<TResult> message, Action<MessageExecuteSetting> configurationSetting) where TResult : class
        {
            var setting = new MessageExecuteSetting();
            configurationSetting.Do(action => action(setting));
            return dispatcher.Query(message, setting);
        }

        public static async Task<TResult> QueryAsync<TResult>(this IDispatcher dispatcher, QueryBase<TResult> message, Action<MessageExecuteSetting> configurationSetting, CancellationToken ct = default) where TResult : class
        {
            var setting = new MessageExecuteSetting();
            configurationSetting.Do(action => action(setting));
            return await dispatcher.QueryAsync(message, setting, ct);
        }

        #endregion
    }
}