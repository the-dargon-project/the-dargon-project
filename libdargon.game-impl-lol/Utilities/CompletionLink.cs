using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Dargon.LeagueOfLegends.Utilities {
   /*
    * var resolution = resolutionChain.New();
    * var compilation = compilationChain.New();
    * var linking = linkingChain.New();
    * resolution.Tail(compilation.StartAndWait);
    * compilation.Tail(linking.StartAndWait);
    * linking.Tail(injection.Inject);
    * resolution.StartAndWait();
    */
   public class CompletionChain {
      private readonly Func<CancellationToken, bool> action;
      private CompletionLink current;

      public CompletionChain(Func<CancellationToken, bool> action) {
         this.action = action;
         this.current = new CompletionLink(this, true, "_sentinel_link_", (cancellationToken) => true);
      }

      public void StartNext(CompletionLink next) {
         var spinner = new SpinWait();
         while (!current._RestartWith(next)) {
            spinner.SpinOnce();
         }
         next._Start();
         current = next;
      }

      public CompletionLink CreateLink(string name) {
         return new CompletionLink(this, false, name, action);
      }
   }

   public class CompletionLink {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
      private readonly List<Action> completionHandlers = new List<Action>();
      private readonly CompletionChain chain;
      private readonly ManualResetEvent completionLatch;
      private readonly string name;
      private readonly Func<CancellationToken, bool> action;
      private CompletionLink previous = null;
      private CompletionLink next = null;
      private Task task;
      private bool isCompletedUninterrupted;

      public CompletionLink(CompletionChain chain, bool isCompleted, string name, Func<CancellationToken, bool> action) {
         this.chain = chain;
         this.completionLatch = new ManualResetEvent(isCompleted);
         this.name = name;
         this.action = action;
      }

      public void _Start() {
         Debug.Print(name + " STarting!");
         task = Task.Run(() => {
            try {
               Debug.Print(name + " Entered task run start!");
               if (action(cancellationTokenSource.Token)) {
                  isCompletedUninterrupted = true;
                  HandleCompletion();
               } else {
                  Debug.Print(name + " got the cancelled =(");
               }
               Debug.Print(name + " UnsTarting!");
            } catch (Exception e) {
               logger.Error($"Threw error in chain link {name}:", e);
            }
         }, cancellationTokenSource.Token);
      }

      public bool _RestartWith(CompletionLink next) {
         if (Interlocked.CompareExchange(ref this.next, next, null) != null) {
            return false;
         } else {
            cancellationTokenSource.Cancel();
            task?.Wait();
            if (!isCompletedUninterrupted) {
               this.next = next;
               next.previous = this;
            }
            return true;
         }
      }

      public void Tail(Action completionHandler) {
         this.completionHandlers.Add(completionHandler);
      }

      public void StartAndWaitForChain() {
         chain.StartNext(this);
         this.WaitForChain();
      }

      private void HandleCompletion() {
         previous?.HandleCompletion();
         completionHandlers.ForEach(x => x.Invoke());
         completionLatch.Set();
      }

      public void Wait() => completionLatch.WaitOne();

      public void WaitForChain() {
         Wait();
         if (!isCompletedUninterrupted) {
            next.Wait();
         }
      }
   }
}
