using System;
using System.Windows.Input;

namespace Dargon.Client.ViewModels.Helpers {
   public sealed class ActionCommand : ICommand {
      private readonly Action<object> action;
      private readonly Predicate<object> predicate;
      public event EventHandler CanExecuteChanged;

      public ActionCommand(Action<object> action)
          : this(action, null) {
      }

      public ActionCommand(Action<object> action, Predicate<object> predicate) {
         if (action == null) {
            throw new ArgumentNullException("action", "You must specify an Action<T>.");
         }

         this.action = action;
         this.predicate = predicate;
      }

      public bool CanExecute(object parameter) {
         if (predicate == null) {
            return true;
         }
         return predicate(parameter);
      }

      public void Execute(object parameter) {
         action(parameter);
      }

      public void Execute() {
         Execute(null);
      }


      //TODO(chris): Remove due to calls to CanExecute are done at an extreme number of times
      event EventHandler ICommand.CanExecuteChanged {
         add { CommandManager.RequerySuggested += value; }
         remove { CommandManager.RequerySuggested -= value; }
      }
   }
}
