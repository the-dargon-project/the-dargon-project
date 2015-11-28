using Dargon.IO;
using Dargon.IO.Components;
using Dargon.Nest;
using Dargon.Nest.Repl;
using Dargon.RADS;
using Dargon.Repl;
using ItzWarty;
using ItzWarty.IO;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace dargon_asset_dumper {
   public static class Program {
      public static void Main(string[] args) {
         Console.WindowWidth = Console.BufferWidth = 120;
         var streamFactory = new StreamFactory();
         IFileSystemProxy fileSystemProxy = new FileSystemProxy(streamFactory);
         DargonNodeFactory dargonNodeFactory = new DargonNodeFactoryImpl();
         var dispatcher = new DispatcherCommand("root");
         dispatcher.RegisterCommand(new ExitCommand());
         dispatcher.RegisterCommand(new LoadSolutionCommand(streamFactory, dargonNodeFactory));
         dispatcher.RegisterCommand(new ChangeDirectoryCommand());
         dispatcher.RegisterCommand(new ListDirectoryCommand());
         dispatcher.RegisterCommand(new PrintWorkingDirectoryCommand());
         dispatcher.RegisterCommand(new AliasCommand("dir", new ListDirectoryCommand()));
         dispatcher.RegisterCommand(new DumpCommand(fileSystemProxy));
         dispatcher.RegisterCommand(new SetWindowWidthCommand());
         dispatcher.Eval(LoadSolutionCommand.kLoadSolutionCommand + " " + (ConfigurationManager.AppSettings?["radsPath"] ?? ""));
         new ReplCore(dispatcher).Run();
      }
   }

   public class SetWindowWidthCommand : ICommand {
      public string Name => "sww";

      public int Eval(string args) {
         string widthString;
         args = Util.NextToken(args, out widthString);

         int width;
         if (int.TryParse(widthString, out width)) {
            Console.WindowWidth = Console.BufferWidth = width;
            return 0;
         }
         return 1;
      }
   }

   public static class DumperUtils {
      public static bool TryResolvePath(string relativePath, out ReadableDargonNode node) {
         relativePath = relativePath.Replace('\\', '/');

         var currentNode = DumperGlobals.CurrentNode;
         while (relativePath.Length > 0) {
            if (relativePath[0] == '/') {
               currentNode = DumperGlobals.RootNode;
               relativePath = relativePath.Substring(1);
            } else {
               var delimiterIndex = relativePath.IndexOf('/');
               string breadcrumbName;
               if (delimiterIndex == -1) {
                  breadcrumbName = relativePath;
                  relativePath = "";
               } else {
                  breadcrumbName = relativePath.Substring(0, delimiterIndex);
                  relativePath = relativePath.Substring(delimiterIndex + 1);
               }

               if (breadcrumbName == ".") {
                  // do nothing
               } else if (breadcrumbName == "..") {
                  var parent = currentNode.Parent;
                  if (parent == null) {
                     Console.Error.WriteLine($"Parent of node {currentNode.Name} does not exist.");
                  } else {
                     currentNode = parent;
                  }
               } else {
                  ReadableDargonNode nextNode;
                  if (!currentNode.TryGetChild(breadcrumbName, out nextNode)) {
                     Console.Error.WriteLine($"Unable to find child {breadcrumbName}.");
                     node = null;
                     return false;
                  } else {
                     currentNode = nextNode;
                  }
               }
            }
         }
         node = currentNode;
         return true;
      }

      public static void PrintUnableToResolvePath(string relativePath) {
         Console.Error.WriteLine($"Unable to resolve path {relativePath}.");
      }
   }

   public class ListDirectoryCommand : ICommand {
      public string Name => "ls";

      public int Eval(string args) {
         string relativePath;
         args = Util.NextToken(args, out relativePath);

         ReadableDargonNode nextNode;
         if (!DumperUtils.TryResolvePath(relativePath, out nextNode)) {
            DumperUtils.PrintUnableToResolvePath(relativePath);
            return 1;
         } else {
            Console.WriteLine("Listing directory " + DumperGlobals.CurrentNode.GetPath());
            PrettyPrint.List(
               nextNode.Children.OrderBy(x => x.Name),
               new PrettyFormatter<ReadableDargonNode> {
                  GetName = (n) => n.Name,
                  GetBackground = (n) => n.Children.Any() ? ConsoleColor.DarkRed : ConsoleHelpers.DefaultBackgroundColor,
                  GetForeground = (n) => n.Children.Any() ? ConsoleColor.White : ConsoleHelpers.DefaultForegroundColor,
               });
            return 0;
         }
      }
   }

   public class PrintWorkingDirectoryCommand : ICommand {
      public string Name => "pwd";

      public int Eval(string args) {
         Console.WriteLine(DumperGlobals.CurrentNode.GetPath());
         return 0;
      }
   }

   public class ChangeDirectoryCommand : ICommand {
      public string Name => "cd";

      public int Eval(string args) {
         string relativePath;
         args = Util.NextToken(args, out relativePath);

         ReadableDargonNode nextNode;
         if (!DumperUtils.TryResolvePath(relativePath, out nextNode)) {
            DumperUtils.PrintUnableToResolvePath(relativePath);
            return 1;
         } else {
            DumperGlobals.CurrentNode = nextNode;
            Console.WriteLine("Switched to directory " + DumperGlobals.CurrentNode.GetPath());
            return 0;
         }
      }
   }

   public class DumpCommand : ICommand {
      private readonly IFileSystemProxy fileSystemProxy;

      public DumpCommand(IFileSystemProxy fileSystemProxy) {
         this.fileSystemProxy = fileSystemProxy;
      }

      public string Name => "dump";

      public int Eval(string args) {
         string dumpDirectory = ConfigurationManager.AppSettings?["dumpPath"] ?? "C:/DargonDump";
         var startNode = DumperGlobals.CurrentNode;
         foreach (var node in startNode.GetLeaves()) {
            Console.Write("Dumping: " + node.GetPath() + " ... ");
            var dataStreamComponent = node.GetComponentOrNull<DataStreamComponent>();
            if (dataStreamComponent == null) {
               Console.WriteLine("Failed. No datastream component.");
               continue;
            }
            using (var stream = dataStreamComponent.CreateRead()) {
               var outputPath = dumpDirectory + node.GetPath();
               fileSystemProxy.PrepareParentDirectory(outputPath);
               using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                  stream.__Stream.CopyTo(fs);
               }
            }
            Console.WriteLine("Done.");
         }
         return 0;
      }
   }

   public static class DumperGlobals {
      public static RiotSolution Solution { set; get; }
      public static ReadableDargonNode RootNode { get; set; }
      public static ReadableDargonNode CurrentNode { get; set; }
   }

   public class LoadSolutionCommand : ICommand {
      public const string kLoadSolutionCommand = "load-solution";
      private readonly IStreamFactory streamFactory;
      private readonly DargonNodeFactory dargonNodeFactory;

      public LoadSolutionCommand(IStreamFactory streamFactory, DargonNodeFactory dargonNodeFactory) {
         this.streamFactory = streamFactory;
         this.dargonNodeFactory = dargonNodeFactory;
      }

      public string Name => kLoadSolutionCommand;

      public int Eval(string args) {
         string radsPath = args;
         while (string.IsNullOrWhiteSpace(radsPath)) {
            Console.Write("Enter RADS Path: ");
            radsPath = Console.ReadLine();
         }
         Console.WriteLine($"Switching to RADS solution at path `{radsPath}`.");
         var riotProjectLoader = new RiotProjectLoader(streamFactory);
         var solution = DumperGlobals.Solution = new RiotSolutionLoader(riotProjectLoader).Load(radsPath);
         DumperGlobals.RootNode = DumperGlobals.CurrentNode = solution.ProjectsByType[RiotProjectType.GameClient].ReleaseManifest.Root;
         return 0;
      }
   }
}
