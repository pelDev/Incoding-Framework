namespace Incoding.Block
{
    #region << Using >>

    using Incoding.CQRS;
    using System.Threading;
    using System.Threading.Tasks;

    #endregion

    public class ChangeDelayToSchedulerStatusByUIDCommand : CommandBase
    {
        protected override void Execute()
        {
            foreach (var delay in Repository.Query(whereSpecification: new DelayToScheduler.Where.ByUID(UID)))
            {
                Dispatcher.Push(new ChangeDelayToSchedulerStatusCommand
                                {
                                        Id = delay.Id,
                                        Status = Status
                                });
            }
        }

        protected override async Task ExecuteAsync(CancellationToken ct = default)
        {
            foreach (var delay in Repository.Query(whereSpecification: new DelayToScheduler.Where.ByUID(UID)))
            {
                await Dispatcher.PushAsync(new ChangeDelayToSchedulerStatusCommand
                {
                    Id = delay.Id,
                    Status = Status
                }, null, ct);
            }
        }

        #region Properties

        public string UID { get; set; }

        public DelayOfStatus Status { get; set; }

        #endregion
    }
}