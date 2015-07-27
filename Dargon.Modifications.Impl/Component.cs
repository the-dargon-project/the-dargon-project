using System.ComponentModel;
using Dargon.PortableObjects;

namespace Dargon.Modifications {
   public interface Component : INotifyPropertyChanged, IPortableObject { }
}