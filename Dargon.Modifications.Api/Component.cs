using Dargon.PortableObjects;
using System.ComponentModel;

namespace Dargon.Modifications {
   public interface Component : INotifyPropertyChanged, IPortableObject {
      void Load(Component component);
   }
}