using System;
using Dargon.Daemon;
using Dargon.Game;
using Dargon.Modifications;
using Dargon.Patcher;

namespace Dargon.LeagueOfLegends.Modifications
{
   public class LeagueModificationCompilationServiceImpl : LeagueModificationOperationServiceBase<ICompilationTask, LeagueModificationCompilationServiceImpl.CompilationTask, LeagueModificationCompilationServiceImpl.CompilationContext>, LeagueModificationCompilationService
   {
      private const string COMPILATION_METADATA_FILE_NAME = "COMPILATION";

      public LeagueModificationCompilationServiceImpl(DaemonService daemonService) 
         : base(daemonService) { }

      public ICompilationTask CompileModification(IModification modification, ModificationTargetType target)
      {
         if (modification.GameType != GameType.LeagueOfLegends) {
            throw new InvalidOperationException("League Modification Compilation Service can only compile League of Legends modifications!");
         }

         logger.Info("Compiling Modification " + modification + " for Target Type " + target);

         var newTask = new CompilationTask(modification);
         AddTask(modification.LocalGuid, newTask, target);
         return newTask;
      }

      protected override CompilationContext CreateContext(CompilationTask task, ModificationTargetType target) { 
         return new CompilationContext(task, target);
      }
      protected override void ProcessTaskContext(CompilationContext context)
      {
         var task = context.Task;
         var modification = task.Modification;
         var repository = new LocalRepository(modification.RootPath);
         using (repository.TakeLock()) {
            string resolutionMetadataFilepath = repository.GetMetadataFilePath(COMPILATION_METADATA_FILE_NAME);
         }
      }

      public class CompilationContext : ITaskContext<CompilationTask>
      {
         private readonly CompilationTask task;
         private readonly ModificationTargetType target;

         public CompilationContext(CompilationTask task, ModificationTargetType target)
         {
            this.task = task;
            this.target = target;
         }

         public CompilationTask Task { get { return task; } }
         public ModificationTargetType Target { get { return target; } }
      }

      public class CompilationTask : ManagableTask, ICompilationTask
      {
         private readonly IModification modification;

         public CompilationTask(IModification modification) : base() { this.modification = modification; }

         public IModification Modification { get { return modification; } }
      }
   }
}