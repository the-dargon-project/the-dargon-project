using System.Collections.Generic;
using Dargon.Daemon;
using Dargon.Game;
using Dargon.Modifications;
using Dargon.Patcher;
using System;
using System.IO;
using ItzWarty;

namespace Dargon.LeagueOfLegends.Modifications
{
   public class LeagueModificationObjectCompilerServiceImpl : LeagueModificationOperationServiceBase<ICompilationTask, LeagueModificationObjectCompilerServiceImpl.CompilationTask, LeagueModificationObjectCompilerServiceImpl.CompilationContext>, LeagueModificationObjectCompilerService
   {
      public const string COMPILATION_METADATA_FILE_NAME = "COMPILATION";

      public LeagueModificationObjectCompilerServiceImpl(DaemonService daemonService) 
         : base(daemonService) { }

      public ICompilationTask CompileObjects(IModification modification, ModificationTargetType target)
      {
         if (modification.GameType != GameType.LeagueOfLegends) {
            throw new InvalidOperationException("League Modification Compilation Service can only compile League of Legends modifications!");
         }

         logger.Info("Compiling Objects for Modification " + modification + " for Target Type " + target);

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
            string compilationMetadataFilepath = repository.GetMetadataFilePath(COMPILATION_METADATA_FILE_NAME);
            string resolutionMetadataFilepath = repository.GetMetadataFilePath(LeagueModificationResolutionServiceImpl.RESOLUTION_METADATA_FILE_NAME); // HACK
            using (var compilationMetadata = new ModificationCompilationTable(compilationMetadataFilepath)) 
            using (var resolutionTable = new ModificationResolutionTable(resolutionMetadataFilepath)) {
               foreach (var indexEntry in repository.EnumerateIndexEntries()) {
                  if (indexEntry.Value.Flags.HasFlag(IndexEntryFlags.Directory)) {
                     continue;
                  }

                  string internalPath = indexEntry.Key;
                  var resolutionEntry = resolutionTable.GetValueOrNull(internalPath);
                  var compilationEntry = compilationMetadata.GetValueOrNull(internalPath);
                  if (resolutionEntry == null || resolutionEntry.Target == ModificationTargetType.Invalid) {
                     logger.Warn("NOT COMPILING UNRESOLVED FILE " + internalPath);
                  } else if (!context.Target.HasFlag(resolutionEntry.Target)) {
                     logger.Warn("NOT COMPILING UNTARGETED FILE " + internalPath);
                  } else {
                     var trueLastModified = repository.GetTrueLastModifiedInternal(internalPath);
                     if (compilationEntry != null && compilationEntry.LastModified == trueLastModified) {
                        logger.Info("OK EXISTING COMPILATION " + internalPath + " TO " + compilationEntry.CompiledFileHash);
                        continue; // already compiled happily
                     }

                     if (resolutionEntry.Target.HasFlag(ModificationTargetType.Game)) {
                        logger.Info("COMPILING FOR GAME " + internalPath + " (index " + indexEntry.Value.RevisionHash + ")");
                        var absolutePath = repository.GetAbsolutePath(internalPath);
                        var contents = File.ReadAllBytes(absolutePath);
                        var compiledFileHash = repository.AddFileObject(contents);
                        compilationMetadata[internalPath] = new ModificationCompilationTable.ModificationCompilationValue(indexEntry.Value.RevisionHash, trueLastModified, compiledFileHash);
                        logger.Info("   => COMPILED TO " + compiledFileHash);
                     } else {
                        logger.Info("COMPILING AIR FILE " + internalPath + " TO NULL HASH");
                        compilationMetadata[internalPath] = new ModificationCompilationTable.ModificationCompilationValue(indexEntry.Value.RevisionHash, trueLastModified, Hash160.Zero);
                     }
                  }
               }
            }
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