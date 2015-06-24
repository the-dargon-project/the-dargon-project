using System;
using System.Collections.Generic;
using System.Linq;
using Dargon.Trinkets.Components;
using ItzWarty;

namespace Dargon.Trinkets {
   public interface DimInstanceContext {
      TConfigurationComponent GetComponentOrNull<TConfigurationComponent>() 
         where TConfigurationComponent : TrinketComponent;
   }

   public class DimInstanceContextImpl : DimInstanceContext {
      private readonly IReadOnlyList<TrinketComponent> components;
      private readonly Dictionary<Type, TrinketComponent> componentsByType;

      public DimInstanceContextImpl(IReadOnlyList<TrinketComponent> components) {
         this.components = components;
         this.componentsByType = components.ToDictionary(c => c.GetType());
      }

      public TConfigurationComponent GetComponentOrNull<TConfigurationComponent>() 
         where TConfigurationComponent : TrinketComponent {
         return (TConfigurationComponent)componentsByType.GetValueOrDefault(typeof(TConfigurationComponent));
      }
   }
}
