namespace Incoding.Block
{
    #region << Using >>

    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Incoding.Block.Logging;
    using Incoding.CQRS;
    using Incoding.Extensions;

    #endregion

    public class StartSchedulerCommand : CommandBase
    {
        public StartSchedulerCommand()
        {
            Conditional = () => true;
            FetchSize = 10;
            Interval = new TimeSpan(0, 0, 0, 1, 0);
            TaskCreationOptions = TaskCreationOptions.LongRunning;
            DelayToStart = TimeSpan.Zero;
        }

        protected override void Execute()
        {
            Action<bool> execute = (isAsync) =>
                                   {
                                       if (DelayToStart.GetValueOrDefault(TimeSpan.Zero) != TimeSpan.Zero)
                                           Thread.Sleep(DelayToStart.GetValueOrDefault());

                                       var isFirstTime = true;
                                       while (true)
                                       {
                                           try
                                           {
                                               while (Conditional())
                                               {
                                                   foreach (var response in Dispatcher.New().Query(new GetExpectedDelayToSchedulerQuery
                                                                                                   {
                                                                                                           FetchSize = FetchSize,
                                                                                                           Date = DateTime.UtcNow,
                                                                                                           Async = isAsync,
                                                                                                           IncludeInProgress = isFirstTime
                                                                                                   }))
                                                   {
                                                       var closureResponse = response;

                                                       Dispatcher.New().Push(new ChangeDelayToSchedulerStatusCommand { Id = closureResponse.Id, Status = DelayOfStatus.InProgress });

                                                       var task = Task.Factory.StartNew(() =>
                                                                                        {
                                                                                            try
                                                                                            {
                                                                                                Stopwatch sw = new Stopwatch();
                                                                                                sw.Start();
                                                                                                Dispatcher.New().Push(closureResponse.Instance);
                                                                                                sw.Stop();

                                                                                                Dispatcher.New().Push(new ChangeDelayToSchedulerStatusCommand
                                                                                                                      {
                                                                                                                              Id = closureResponse.Id,
                                                                                                                              Status = DelayOfStatus.Success,
                                                                                                                              Description = "Executed in {0} sec of {1} timeout".F(sw.Elapsed.TotalSeconds, closureResponse.TimeOut)
                                                                                                                      });
                                                                                            }
                                                                                            catch (Exception ex)
                                                                                            {
                                                                                                if (!string.IsNullOrWhiteSpace(Log_Debug))
                                                                                                    LoggingFactory.Instance.LogException(Log_Debug, ex);

                                                                                                Dispatcher.New().Push(new ChangeDelayToSchedulerStatusCommand
                                                                                                                      {
                                                                                                                              Id = closureResponse.Id,
                                                                                                                              Status = DelayOfStatus.Error,
                                                                                                                              Description = ex.ToString()
                                                                                                                      });
                                                                                            }
                                                                                        }, TaskCreationOptions.LongRunning);

                                                       if (!isAsync)
                                                           task.Wait(response.TimeOut);
                                                   }
                                                   isFirstTime = false;
                                                   GetExpectedDelayToSchedulerQuery.LastDate = Dispatcher.New().Query(new GetExpectedDelayToSchedulerQuery.GetLastDateQuery()
                                                                                                                      {
                                                                                                                              Date = DateTime.UtcNow,
                                                                                                                              Async = true
                                                                                                                      });
                                                   Thread.Sleep(Interval);
                                               }
                                           }
                                           catch (Exception ex)
                                           {
                                               if (ex is ThreadAbortException)
                                                   Thread.ResetAbort(); // cancel any abort to prevent stop scheduler

                                               if (!string.IsNullOrWhiteSpace(Log_Debug))
                                                   LoggingFactory.Instance.LogException(Log_Debug, ex);
                                           }
                                           Thread.Sleep(5.Seconds());
                                       }
                                   };

            Task.Factory.StartNew(() => execute(true), TaskCreationOptions);
            Task.Factory.StartNew(() => execute(false), TaskCreationOptions);
        }

        protected override async Task ExecuteAsync(CancellationToken ct = default)
        {
            Func<bool, CancellationToken, Task> execute = async (isAsync, cancellationToken) =>
            {
                // Initial delay with cancellation support
                if (DelayToStart.GetValueOrDefault(TimeSpan.Zero) != TimeSpan.Zero)
                    await Task.Delay(DelayToStart.GetValueOrDefault(), cancellationToken).ConfigureAwait(false);

                var isFirstTime = true;
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        while (Conditional() && !cancellationToken.IsCancellationRequested)
                        {
                            // Query for scheduled tasks asynchronously
                            var responses = await Dispatcher.New().QueryAsync(new GetExpectedDelayToSchedulerQuery
                            {
                                FetchSize = FetchSize,
                                Date = DateTime.UtcNow,
                                Async = isAsync,
                                IncludeInProgress = isFirstTime
                            }, null, cancellationToken).ConfigureAwait(false);

                            foreach (var response in responses)
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                var closureResponse = response;

                                // Mark as in progress
                                await Dispatcher.New().PushAsync(new ChangeDelayToSchedulerStatusCommand
                                {
                                    Id = closureResponse.Id,
                                    Status = DelayOfStatus.InProgress
                                }, executeSetting: null, ct: cancellationToken).ConfigureAwait(false);

                                // Create task for execution with proper async handling
                                var task = Task.Run(async () =>
                                {
                                    try
                                    {
                                        var sw = Stopwatch.StartNew();

                                        // Execute the scheduled command/query asynchronously
                                        await Dispatcher.New().PushAsync(closureResponse.Instance, executeSetting: null, ct: cancellationToken).ConfigureAwait(false);

                                        sw.Stop();

                                        // Mark as successful
                                        await Dispatcher.New().PushAsync(new ChangeDelayToSchedulerStatusCommand
                                        {
                                            Id = closureResponse.Id,
                                            Status = DelayOfStatus.Success,
                                            Description = "Executed in {0} sec of {1} timeout".F(sw.Elapsed.TotalSeconds, closureResponse.TimeOut)
                                        }, executeSetting: null, ct: cancellationToken).ConfigureAwait(false);
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        // Handle cancellation - mark as error
                                        await Dispatcher.New().PushAsync(new ChangeDelayToSchedulerStatusCommand
                                        {
                                            Id = closureResponse.Id,
                                            Status = DelayOfStatus.Error,
                                            Description = "Task was cancelled"
                                        }, executeSetting: null, CancellationToken.None).ConfigureAwait(false); // Use None to ensure status update

                                        throw; // Re-throw to maintain cancellation semantics
                                    }
                                    catch (Exception ex)
                                    {
                                        if (!string.IsNullOrWhiteSpace(Log_Debug))
                                            LoggingFactory.Instance.LogException(Log_Debug, ex);

                                        await Dispatcher.New().PushAsync(new ChangeDelayToSchedulerStatusCommand
                                        {
                                            Id = closureResponse.Id,
                                            Status = DelayOfStatus.Error,
                                            Description = ex.ToString()
                                        }, executeSetting: null, CancellationToken.None).ConfigureAwait(false); // Use None to ensure error logging
                                    }
                                }, cancellationToken);

                                // Handle sync vs async execution
                                if (!isAsync)
                                {
                                    try
                                    {
                                        // For sync mode, wait with timeout
                                        using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                                        {
                                            timeoutCts.CancelAfter(response.TimeOut);
                                            await task.ConfigureAwait(false);
                                        }
                                    }
                                    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                                    {
                                        // Timeout occurred (not general cancellation)
                                        await Dispatcher.New().PushAsync(new ChangeDelayToSchedulerStatusCommand
                                        {
                                            Id = closureResponse.Id,
                                            Status = DelayOfStatus.Error,
                                            Description = $"Task timed out after {response.TimeOut}ms"
                                        }, executeSetting: null, CancellationToken.None).ConfigureAwait(false);
                                    }
                                }
                                // For async mode, let the task run in background (fire and forget)
                            }

                            isFirstTime = false;

                            // Update last date asynchronously
                            GetExpectedDelayToSchedulerQuery.LastDate = await Dispatcher.New().QueryAsync(
                                new GetExpectedDelayToSchedulerQuery.GetLastDateQuery()
                                {
                                    Date = DateTime.UtcNow,
                                    Async = true
                                }, null, cancellationToken).ConfigureAwait(false);

                            // Interval delay with cancellation support
                            await Task.Delay(Interval, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Cancellation requested - exit gracefully
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (!string.IsNullOrWhiteSpace(Log_Debug))
                            LoggingFactory.Instance.LogException(Log_Debug, ex);
                    }

                    try
                    {
                        // Brief pause before retrying (with cancellation support)
                        await Task.Delay(5.Seconds(), cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            };

            // Start both async and sync schedulers with the provided cancellation token
            var asyncTask = execute(true, ct);
            var syncTask = execute(false, ct);

            await Task.WhenAll(asyncTask, syncTask).ConfigureAwait(false);
        }

        #region Properties

        public TimeSpan? DelayToStart { get; set; }

        public string Log_Debug { get; set; }

        public TimeSpan Interval { get; set; }

        public Func<bool> Conditional { get; set; }

        public int FetchSize { get; set; }

        public TaskCreationOptions TaskCreationOptions { get; set; }

        #endregion
    }
}