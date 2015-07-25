using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dargon.Client.Annotations;
using Dargon.Client.Controllers;
using Dargon.Client.ViewModels.Helpers;
using Dargon.Client.Views;
using ItzWarty;

namespace Dargon.Client.ViewModels {
   public class ModificationImportViewModel : INotifyPropertyChanged {
      private readonly ModificationImportController modificationImportController;
      private readonly ModificationImportWindow importWindow;
      private readonly ModificationImportEntryViewModelBase rootNodeViewModel;
      private bool isEnabled = true;

      public ModificationImportViewModel(ModificationImportController modificationImportController, ModificationImportWindow importWindow, ModificationImportEntryViewModelBase rootNodeViewModel) {
         this.modificationImportController = modificationImportController;
         this.importWindow = importWindow;
         this.rootNodeViewModel = rootNodeViewModel;
      }

      public IEnumerable<ModificationImportEntryViewModelBase> RootNodeAsCollection => rootNodeViewModel.Wrap();
      public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; OnPropertyChanged(); } }

      public ICommand ImportLegacyModificationAndCloseWindow => new ActionCommand((x) => {
         var modificationName = importWindow.ModificationNameTextBox.Text;
         if (modificationName.Length == 0) {
            MessageBox.Show("Modification Name must be filled in!");
            return;
         }

         IsEnabled = false;
         ThreadPool.QueueUserWorkItem((y) => {
            try {
               modificationImportController.ImportLegacyModification(
                  modificationName,
                  rootNodeViewModel.Path,
                  rootNodeViewModel.EnumerateFileNodes().Select(node => node.Path).ToArray()
               );
               Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => {
                  MessageBox.Show(importWindow, $"Imported Modification {modificationName}!");
                  importWindow.Close();
               }));
            } catch (Exception e) {
               MessageBox.Show(e.ToString());
               IsEnabled = true;
            }
         });
      });

      public event PropertyChangedEventHandler PropertyChanged;

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}
