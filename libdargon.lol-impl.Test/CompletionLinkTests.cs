using NMockito;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dargon.LeagueOfLegends.Utilities;
using Xunit;

namespace Dargon.LeagueOfLegends {
   public class CompletionLinkTests : NMockitoInstance {
      [Fact]
      public void Run() {
         var resolutionChain = new CompletionChain(cancellationToken => {
            Debug.Print("Resolving");
            var task = Task.Delay(2000);
            try {
               task.Wait(cancellationToken);
               Debug.Print("Resolution Completed");
               return true;
            } catch (OperationCanceledException e) {
               return false;
            }
         });
         var compilationChain = new CompletionChain(cancellationToken => {
            Debug.Print("Compiling");
            var task = Task.Delay(1500);
            try {
               task.Wait(cancellationToken);
               Debug.Print("Compilation Completed");
               return true;
            } catch (OperationCanceledException e) {
               return false;
            }
         });
         var injectionChain = new CompletionChain(cancellationToken => {
            Debug.Print("Injecting");
            var task = Task.Delay(1000);
            try {
               task.Wait(cancellationToken);
               Debug.Print("Linking Completed");
               return true;
            } catch (OperationCanceledException e) {
               return false;
            }
         });

         var resolutionLink1 = resolutionChain.CreateLink("RL1");
         var compilationLink1 = compilationChain.CreateLink("CL1");
         var injectionLink1 = injectionChain.CreateLink("IL1");
         resolutionLink1.Tail(compilationLink1.StartAndWaitForChain);
         compilationLink1.Tail(injectionLink1.StartAndWaitForChain);

         var resolutionLink2 = resolutionChain.CreateLink("RL2");
         var compilationLink2 = compilationChain.CreateLink("CL2");
         var injectionLink2 = injectionChain.CreateLink("IL2");
         resolutionLink2.Tail(compilationLink2.StartAndWaitForChain);
         compilationLink2.Tail(injectionLink2.StartAndWaitForChain);

         resolutionChain.StartNext(resolutionLink1);

         Thread.Sleep(1000);

         resolutionChain.StartNext(resolutionLink2);

         resolutionLink1.Wait();
         compilationLink1.Wait();
         injectionLink1.Wait();

         resolutionLink2.Wait();
         compilationLink2.Wait();
         injectionLink2.Wait();
      }
   }
}
