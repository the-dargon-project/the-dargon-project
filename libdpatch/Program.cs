using ItzWarty;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Dargon.Patcher
{
   public class Program
   {
      public static void Main()
      {
         Console.WindowWidth = Console.BufferWidth = Console.LargestWindowWidth - 30;
         Console.WindowHeight = Console.LargestWindowHeight - 5;

         var path = "C:/dpm-repositories/test";
         Directory.Delete(path, true);
         Util.PrepareDirectory(path);
         Environment.CurrentDirectory = path;
         var program = new Program(Environment.CurrentDirectory);
         program.PrintStatus();

         program.Initialize();
         program.PrintStatus();

         Console.ForegroundColor = ConsoleColor.Cyan;
         Console.WriteLine("Creating Program.cs...");
         File.WriteAllBytes("Program.cs", Encoding.UTF8.GetBytes("class Program { public static void Main() { } }"));
         program.PrintStatus();

         program.Add("Program.cs");
         program.PrintStatus();

         var initialCommit = program.Commit("Initial Commit.");
         program.PrintStatus();

         Console.ForegroundColor = ConsoleColor.Gray;
         Console.WriteLine("Modifying Program.cs...");
         File.WriteAllBytes("Program.cs", Encoding.UTF8.GetBytes("using System; class Program { public static void Main() { Console.WriteLine(\"Hello, World!\"); } }"));
         File.WriteAllBytes("Fruit.cs", Encoding.UTF8.GetBytes("using System; public class Fruit { }"));
         File.WriteAllBytes("Apple.cs", Encoding.UTF8.GetBytes("using System; public class Apple : Fruit { }"));
         program.PrintStatus();

         program.Update("Program.cs");
         program.Add("Fruit.cs");
         program.Add("Apple.cs");
         program.PrintStatus();

         var entryPointCommit = program.Commit("Entry Point prints \"Hello, World!\", added Fruit and Apple.");
         program.PrintStatus();

         program.PrintLinearGraph();
      }

      private readonly LocalRepository repository;

      public Program(string path)
      {
         repository = new LocalRepository(path);
      }

      public LocalRepository Repository { get { return repository; } }

      public void Initialize()
      {
         Console.ForegroundColor = ConsoleColor.White;
         Console.WriteLine("Initializing dpm Repository at " + repository.Root);
         Console.ForegroundColor = ConsoleColor.Gray;
         repository.Initialize();
         Console.WriteLine();
      }

      public void Add(string path)
      {
         Console.ForegroundColor = ConsoleColor.White;
         Console.WriteLine("Adding " + path + " to index");
         Console.ForegroundColor = ConsoleColor.Gray;
         repository.AddFile(path);
         Console.WriteLine();
      }

      public void Update(string path)
      {
         Console.ForegroundColor = ConsoleColor.White;
         Console.WriteLine("Updating " + path + " revision in index");
         Console.ForegroundColor = ConsoleColor.Gray;
         repository.UpdateFile(path);
         Console.WriteLine();
      }

      public Hash160 Commit(string message)
      {
         Console.ForegroundColor = ConsoleColor.White;
         Console.WriteLine("Committing changes in index to local history.");
         Console.ForegroundColor = ConsoleColor.Gray;
         var result = repository.Commit(message);
         Console.WriteLine();
         return result;
      }

      public void PrintStatus()
      {
         Console.ForegroundColor = ConsoleColor.White;
         Console.WriteLine("Printing Status of Repository " + repository.Root);
         if (!repository.GetIsInitialized()) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(" Repository not initialized!");
            Console.WriteLine();
            return;
         } else {
            Console.WriteLine(" root at " + repository.GetRootHash().ToString("X"));
            var head = repository.Head;
            Console.WriteLine(" head at " + head.Type + " " + head.Value);
//            if (head.Type == LocalRepository.HeadType.Branch) {
               Console.WriteLine("  " + head.Value + " is at commit " + repository.GetBranchCommitHash(head.Value).ToString("x"));
//               repository.GetCommitRootHash(repository.GetBranchCommitHash(head.Value));
//            }
         }

         var changes = repository.EnumerateChangedFiles();
         var changesBinnedByStaged = changes.GroupBy(change => change.Value & ChangeType.Staged);
         foreach (var bin in changesBinnedByStaged) {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" " + (bin.Key == ChangeType.Staged ? "STAGED CHANGES" : "UNSTAGED CHANGES"));
            foreach (var changedEntry in bin) {
               string operation = "";
               if (changedEntry.Value.HasFlag(ChangeType.Added)) {
                  operation = "ADD";
                  Console.ForegroundColor = ConsoleColor.Green;
               } else if (changedEntry.Value.HasFlag(ChangeType.Removed)) {
                  operation = "REM";
                  Console.ForegroundColor = ConsoleColor.Red;
               } else {
                  operation = "MOD";
                  Console.ForegroundColor = ConsoleColor.Yellow;
               }
               Console.WriteLine("  " + operation + " " + changedEntry.Key);
            }
         }

         Console.ForegroundColor = ConsoleColor.White;
         Console.WriteLine(" INDEX");
         foreach (var entry in repository.EnumerateIndexEntries()) {
            Console.WriteLine("  \"" + entry.Key + "\" " + entry.Value.RevisionHash.ToString("X") + " " + entry.Value.LastModified + " " + entry.Value.Flags);
         }

         Console.ForegroundColor = ConsoleColor.White;
         Console.WriteLine(" OBJECTS");
         foreach (var key in repository.EnumerateObjectStoreKeys()) {
            Console.WriteLine("  " + key.ToString("X"));
         }
         Console.WriteLine();
      }

      private void PrintLinearGraph() { repository.PrintLinearGraph(); }
   }
}
