using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Tasks;

namespace AppleTv
{
    public class ChannelUpdateScheduledTask : IScheduledTask, IConfigurableScheduledTask
    {
        private ITaskManager TaskManager { get; set; }

        public ChannelUpdateScheduledTask(ITaskManager taskMan)
        {
            TaskManager = taskMan;
        }
        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            foreach (var t in TaskManager.ScheduledTasks)
            {
                if (t.Name == "Refresh Internet Channels")
                {
                    await TaskManager.Execute(t, new TaskOptions());
                }
            }
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return new[]
            {
                new TaskTriggerInfo
                {
                    Type          = TaskTriggerInfo.TriggerInterval,
                    IntervalTicks = TimeSpan.FromDays(1).Ticks
                }
            };
        }

        public string Name        => "Update AppleTV Channel";
        public string Key         => "AppleTvChannel";
        public string Description => "Create/Update channel from trakt list.";
        public string Category    => "Library";
        public bool IsHidden      => false;
        public bool IsEnabled     => true;
        public bool IsLogged      => true;
    }
}