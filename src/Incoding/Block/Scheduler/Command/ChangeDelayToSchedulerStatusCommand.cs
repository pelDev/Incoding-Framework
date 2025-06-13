namespace Incoding.Block
{
    #region << Using >>

    using Incoding.CQRS;
    using System.Threading;
    using System.Threading.Tasks;

    #endregion

    public class ChangeDelayToSchedulerStatusCommand : CommandBase
    {
        protected override void Execute()
        {
            var delay = Repository.GetById<DelayToScheduler>(Id);
            delay.Status = Status;
            delay.Description = Description;

            if (Status == DelayOfStatus.Success && delay.Recurrence != null)
            {
                var recurrency = new GetRecurrencyDateQuery
                {
                    EndDate = delay.Recurrence.EndDate,
                    RepeatCount = delay.Recurrence.RepeatCount - 1,
                    RepeatDays = delay.Recurrence.RepeatDays,
                    RepeatInterval = delay.Recurrence.RepeatInterval,
                    StartDate = delay.StartsOn,
                    Type = delay.Recurrence.Type
                };
                recurrency.NowDate = delay.StartsOn; // calculate next start depending on previously calculated start (to run every day at exactly same time for example)
                recurrency.StartDate = Dispatcher.Query(recurrency);
                if (!recurrency.StartDate.HasValue)
                    return;
                Dispatcher.Push(new AddDelayToSchedulerCommand(delay)
                                {
                                        UID = delay.UID,
                                        Priority = delay.Priority,                                                                               
                                        Recurrency = recurrency,
                                });
            }
        }

        protected override async Task ExecuteAsync(CancellationToken ct = default)
        {
            var delay = Repository.GetById<DelayToScheduler>(Id);
            delay.Status = Status;
            delay.Description = Description;

            if (Status == DelayOfStatus.Success && delay.Recurrence != null)
            {
                var recurrency = new GetRecurrencyDateQuery
                {
                    EndDate = delay.Recurrence.EndDate,
                    RepeatCount = delay.Recurrence.RepeatCount - 1,
                    RepeatDays = delay.Recurrence.RepeatDays,
                    RepeatInterval = delay.Recurrence.RepeatInterval,
                    StartDate = delay.StartsOn,
                    Type = delay.Recurrence.Type
                };
                recurrency.NowDate = delay.StartsOn; // calculate next start depending on previously calculated start (to run every day at exactly same time for example)
                recurrency.StartDate = await Dispatcher.QueryAsync(recurrency, null, ct);
                if (!recurrency.StartDate.HasValue)
                    return;
                await Dispatcher.PushAsync(new AddDelayToSchedulerCommand(delay)
                {
                    UID = delay.UID,
                    Priority = delay.Priority,
                    Recurrency = recurrency,
                }, null, ct);
            }
        }

        #region Properties

        public string Id { get; set; }

        public DelayOfStatus Status { get; set; }

        public string Description { get; set; }

        #endregion
    }
}