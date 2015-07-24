using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dargon.Client.Annotations;
using Dargon.Client.ViewModels.Helpers;

namespace Dargon.Client.ViewModels {
   public class ModificationViewModel : INotifyPropertyChanged {
      private string name;
      private string author;
      private ModificationStatus status;
      private ModificationType type;
      public event PropertyChangedEventHandler PropertyChanged;

      public string Name { get { return name; } set { name = value; OnPropertyChanged(); } }
      public string Author { get { return author; } set { author = value; OnPropertyChanged(); } }
      public ModificationStatus Status { get { return status; } set { status = value; OnPropertyChanged(); } }
      public ModificationType Type { get { return type; } set { type = value; OnPropertyChanged(); } }

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }

   [Flags]
   public enum ModificationStatus {
      Enabled = 0x01,
      Disabled = 0x02,
      Broken = 0x04,
      UpdateAvailable = 0x08
   }

   [Flags]
   public enum ModificationType : byte {
      Map = 0x01,
      Champion = 0x02,
      Ward = 0x04,
      UI = 0x08,
      Other = 0x10,
      All = 0xFF
   }
}
