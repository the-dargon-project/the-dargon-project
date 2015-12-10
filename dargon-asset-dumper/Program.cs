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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Dargon.DDS;
using Dargon.Modifications;

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
         dispatcher.RegisterCommand(new CreateLevelProjectCommand(fileSystemProxy));
         dispatcher.RegisterCommand(new BuildLevelProjectCommand(fileSystemProxy));
         dispatcher.Eval(LoadSolutionCommand.kLoadSolutionCommand + " " + (ConfigurationManager.AppSettings?["radsPath"] ?? ""));
         new ReplCore(dispatcher).Run();
      }
   }

   public class CreateLevelProjectCommand : ICommand {
      private readonly IFileSystemProxy fileSystemProxy;

      public CreateLevelProjectCommand(IFileSystemProxy fileSystemProxy) {
         this.fileSystemProxy = fileSystemProxy;
      }

      public string Name => "create-level-project";

      public int Eval(string args) {
         string name, mapId;
         args = Util.NextToken(args, out name);
         args = Util.NextToken(args, out mapId);

         var projectDirectory = $"C:/DargonDump/{name}";
         var devDirectory = $"{projectDirectory}/dev";
         var startNode = Globals.RootNode.GetRelativeOrNull($"/LEVELS/{mapId}");
         foreach (var node in startNode.GetLeaves()) {
            if (node.Name.StartsWith("2x_") || node.Name.StartsWith("4x_")) {
               continue;
            }
            var nodePath = node.GetPath();
            Console.Write("Dumping: " + nodePath + " ... ");
            var dataStreamComponent = node.GetComponentOrNull<DataStreamComponent>();
            if (dataStreamComponent == null) {
               Console.WriteLine("Failed. No datastream component.");
               continue;
            }
            var outputPath = devDirectory + "/" + nodePath;
            if (File.Exists(outputPath)) {
               Console.WriteLine("Already exists.");
            } else {
               using (var stream = dataStreamComponent.CreateRead()) {
                  fileSystemProxy.PrepareParentDirectory(outputPath);
                  using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                     stream.__Stream.CopyTo(fs);
                  }
               }
               Console.WriteLine("Done.");
            }
         }

         // convert all dds to pngs
         var converter = new TextureConverterFactory().Create();
         foreach (var filePath in Directory.EnumerateFiles(devDirectory, "*", SearchOption.AllDirectories)) {
            if (!new FileInfo(filePath).Name.EndsWith(".dds", StringComparison.OrdinalIgnoreCase)) continue;
            var bitmap = converter.ConvertToBitmap(filePath);
            File.Delete(filePath);
            bitmap.Save(filePath.Substring(0, filePath.Length - 4) + ".png", ImageFormat.Png);
         }

         // combine grnd_terrain_ dds files (5x5 grid)
         var sceneTexturesDirectory = Path.Combine(devDirectory, "LEVELS", mapId, "scene", "textures");
         var groundTerrainTexturePaths = Directory.EnumerateFiles(sceneTexturesDirectory, "grnd_terrain_*.png", SearchOption.TopDirectoryOnly).OrderBy(x => x).ToArray();
         var bitmaps = groundTerrainTexturePaths.Select(converter.ConvertToBitmap).ToArray();
         var n = (int)Math.Sqrt(bitmaps.Length);
         var stitchedBitmap = new Bitmap(bitmaps[0].Width * n, bitmaps[0].Height * n);
         using (var g = Graphics.FromImage(stitchedBitmap)) {
            for (var y = 0; y < n; y++) {
               for (var x = 0; x < n; x++) {
                  var bitmap = bitmaps[y * n + x];
                  g.DrawImage(bitmap, new Point(bitmap.Width * x, bitmap.Height * y));
               }
            }
         }
         groundTerrainTexturePaths.ForEach(File.Delete);
         stitchedBitmap.Save(Path.Combine(sceneTexturesDirectory, "grnd_terrain.png"));

         Globals.CurrentProject = args;
         return 0;
      }
   }

   public class BuildLevelProjectCommand : ICommand {
      private readonly IFileSystemProxy fileSystemProxy;

      public string Name => "build-level-project";

      public BuildLevelProjectCommand(IFileSystemProxy fileSystemProxy) {
         this.fileSystemProxy = fileSystemProxy;
      }

      public int Eval(string args) {
         string name, mapId;
         args = Util.NextToken(args, out name);
         args = Util.NextToken(args, out mapId);

         var projectDirectory = $"C:/DargonDump/{name}";
         var devDirectory = Path.GetFullPath($"{projectDirectory}/dev");
         var outDirectory = Path.GetFullPath($"{projectDirectory}/out");

         // copy all files to out directory
//         if (Directory.Exists(outDirectory)) {
//            Directory.Delete(outDirectory, true);
//         }
//
         foreach (var filePath in Directory.EnumerateFiles(devDirectory, "*", SearchOption.AllDirectories)) {
            var outputPath = outDirectory + filePath.Substring(devDirectory.Length);
            fileSystemProxy.PrepareParentDirectory(outputPath);
            if (!fileSystemProxy.Exists(outputPath)) {
               fileSystemProxy.CopyFile(filePath, outputPath);
            }
         }

         // break up grnd_terrain.png
         var sceneTexturesDirectory = Path.Combine(outDirectory, "LEVELS", mapId, "scene", "textures");
         var groundTerrainBitmapPath = Path.Combine(sceneTexturesDirectory, "grnd_terrain.png");
         using (var groundTerrainBitmap = Image.FromFile(groundTerrainBitmapPath)) {
            int n = 5;
            for (var y = 0; y < n; y++) {
               for (var x = 0; x < n; x++) {
                  var sliceBitmap = new Bitmap(groundTerrainBitmap.Width / n, groundTerrainBitmap.Height / n);
                  using (var g = Graphics.FromImage(sliceBitmap)) {
                     g.DrawImage(
                        groundTerrainBitmap,
                        new Rectangle(0, 0, sliceBitmap.Width, sliceBitmap.Height),
                        new Rectangle(sliceBitmap.Width * x, sliceBitmap.Height * y, sliceBitmap.Width, sliceBitmap.Height),
                        GraphicsUnit.Pixel);
                  }
                  sliceBitmap.Save(Path.Combine(sceneTexturesDirectory, $"grnd_terrain_{(char)('a' + y * n + x)}.png"));
               }
            }
         }
         File.Delete(groundTerrainBitmapPath);

         // scale all images to 2x and 4x (downscaled) reps
         foreach (var filePath in Directory.EnumerateFiles(outDirectory, "*", SearchOption.AllDirectories).ToArray()) {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
            using (var sourceBitmap = Bitmap.FromFile(filePath)) {
               using (var halfBitmap = new Bitmap(sourceBitmap, new Size(sourceBitmap.Width / 2, sourceBitmap.Height / 2))) {
                  halfBitmap.Save(fileInfo.DirectoryName + "/2x_" + fileInfo.Name);
               }
               using (var halfBitmap = new Bitmap(sourceBitmap, new Size(sourceBitmap.Width / 4, sourceBitmap.Height / 4))) {
                  halfBitmap.Save(fileInfo.DirectoryName + "/4x_" + fileInfo.Name);
               }
            }
         }

         // convert all pngs to dds
         var converter = new TextureConverterFactory().Create();
         foreach (var filePath in Directory.EnumerateFiles(outDirectory, "*", SearchOption.AllDirectories)) {
            if (!new FileInfo(filePath).Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
            var outputPath = filePath.Substring(0, filePath.Length - 4) + ".dds";
            Console.WriteLine("Convert " + filePath + " => " + outputPath);
            converter.ConvertAndSaveToTexture(filePath, outputPath);
            File.Delete(filePath);
         }

         return 0;
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

         var currentNode = Globals.CurrentNode;
         while (relativePath.Length > 0) {
            if (relativePath[0] == '/') {
               currentNode = Globals.RootNode;
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
            Console.WriteLine("Listing directory " + Globals.CurrentNode.GetPath());
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
         Console.WriteLine(Globals.CurrentNode.GetPath());
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
            Globals.CurrentNode = nextNode;
            Console.WriteLine("Switched to directory " + Globals.CurrentNode.GetPath());
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
         var startNode = Globals.CurrentNode;
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

   public static class DumpOperations {

   }

   public static class Globals {
      public static RiotSolution Solution { set; get; }
      public static ReadableDargonNode RootNode { get; set; }
      public static ReadableDargonNode CurrentNode { get; set; }
      public static string CurrentProject { get; set; }
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
         var solution = Globals.Solution = new RiotSolutionLoader(riotProjectLoader).Load(radsPath);
         Globals.RootNode = Globals.CurrentNode = solution.ProjectsByType[RiotProjectType.GameClient].ReleaseManifest.Root;
         return 0;
      }
   }
}
