using Dargon.Processes.Watching;
using NLog;

namespace Dargon.FinalFantasyXIII.Processes
{
   public class FFXIIIProcessWatcherServiceImpl
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly ProcessWatcherService processWatcherService;

      public event FFXIIIProcessDetectedHandler LauncherLaunched;
      public const string kLauncherProcessNameLower = "ffxiiilauncher.exe";

      public event FFXIIIProcessDetectedHandler GameLaunched;
      public const string kGameProcessNameLower = "ffxiiiimg.exe";

      public FFXIIIProcessWatcherServiceImpl(ProcessWatcherService processWatcherService) { this.processWatcherService = processWatcherService; }

      public void Initialize() { processWatcherService.Subscribe(HandleProcessCreated, new[] { kLauncherProcessNameLower, kGameProcessNameLower }); }

      public void HandleProcessCreated(CreatedProcessDescriptor desc)
      {
         var lowerProcessName = desc.ProcessName.ToLower();

         FFXIIIProcessDetectedHandler @event = null;
         FFXIIIProcessType processType = FFXIIIProcessType.Invalid;

         if (lowerProcessName.Contains(kLauncherProcessNameLower)) {
            @event = LauncherLaunched;
            processType = FFXIIIProcessType.Launcher;
         } else if (lowerProcessName.Contains(kGameProcessNameLower)) {
            @event = GameLaunched;
            processType = FFXIIIProcessType.Game;
         }

         logger.Info((@event == null) + " " + lowerProcessName + " " + processType);

         if (processType != FFXIIIProcessType.Invalid) {
            logger.Info(lowerProcessName);
            var capture = @event;
            if (capture != null && processType != FFXIIIProcessType.Invalid) {
               capture(new FFXIIIProcessDetectedArgs(processType, desc));
            }
         }
      }
   }
}