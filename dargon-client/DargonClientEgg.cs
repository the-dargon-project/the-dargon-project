using System;
using System.Collections.ObjectModel;
using Dargon.Client.Views;
using Dargon.Nest.Egg;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dargon.Client.ViewModels;

namespace Dargon.Client {
   public class DargonClientEgg : INestApplicationEgg {
      public NestResult Start(IEggParameters parameters) {
         var userInterfaceThread = new Thread(UserInterfaceThreadStart);
         userInterfaceThread.SetApartmentState(ApartmentState.STA);
         userInterfaceThread.Start();
         return NestResult.Success;
      }

      private void UserInterfaceThreadStart() {
         var application = Application.Current ?? new Application();
         var dispatcher = application.Dispatcher;
         var window = new MainWindow();
         ObservableCollection<ModificationViewModel> modifications = new ObservableCollection<ModificationViewModel>();
         modifications.Add(new ModificationViewModel {
            Name = "Something Ezreal",
            Author = "Herp",
            Status = ModificationStatus.Disabled,
            Type = ModificationType.Champion
         });
         modifications.Add(new ModificationViewModel {
            Name = "SR But Better",
            Author = "Lerp",
            Status = ModificationStatus.Enabled,
            Type = ModificationType.Map
         });
         modifications.Add(new ModificationViewModel {
            Name = "Warty the Ward",
            Author = "ijofgsdojmn",
            Status = ModificationStatus.Enabled,
            Type = ModificationType.Ward
         });
         modifications.Add(new ModificationViewModel {
            Name = "ItBlinksAlot UI",
            Author = "23erp",
            Status = ModificationStatus.UpdateAvailable,
            Type = ModificationType.UI
         });
         modifications.Add(new ModificationViewModel {
            Name = "Something Else",
            Author = "Perp",
            Status = ModificationStatus.Broken,
            Type = ModificationType.Other
         });
         var rootViewModel = new RootViewModel(modifications);
         window.DataContext = rootViewModel;
         application.Run(window);
      }

      public NestResult Shutdown() {
         return NestResult.Success;
      }
   }
}
