using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Modifications {
   [ModificationComponent(ComponentOrigin.Local, "ENABLED")]
   public class EnabledComponent : Component {
      private const int kVersion = 0;

      private bool isEnabled;

      public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; OnPropertyChanged(); } }

      public void Serialize(IPofWriter writer) {
         writer.WriteS32(0, kVersion);
         writer.WriteBoolean(1, IsEnabled);
      }

      public void Deserialize(IPofReader reader) {
         var version = reader.ReadS32(0);
         IsEnabled = reader.ReadBoolean(1);

         Trace.Assert(version == kVersion, "version == kVersion");
      }

      public void Load(Component component) {
         var other = (EnabledComponent)component;
         this.IsEnabled = other.IsEnabled;
      }

      public event PropertyChangedEventHandler PropertyChanged;

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}
