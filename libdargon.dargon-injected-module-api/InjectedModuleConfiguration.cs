using System;
using System.Linq;
using Dargon.InjectedModule.Components;
using ItzWarty;
using System.Collections.Generic;

namespace Dargon.InjectedModule
{
   public interface IInjectedModuleConfiguration
   {
      BootstrapConfiguration GetBootstrapConfiguration();

      TConfigurationComponent GetComponentOrNull<TConfigurationComponent>()
         where TConfigurationComponent : IConfigurationComponent;
   }

   public class InjectedModuleConfiguration : IInjectedModuleConfiguration
   {
      private readonly IReadOnlyList<IConfigurationComponent> components;
      private readonly BootstrapConfiguration bootstrapConfiguration;
      private readonly Dictionary<Type, IConfigurationComponent> componentsByType;

      public InjectedModuleConfiguration(IReadOnlyList<IConfigurationComponent> components)
      {
         this.components = components;
         this.bootstrapConfiguration = components.Aggregate(new BootstrapConfigurationBuilder(), (builder, component) => component.AmendBootstrapConfiguration(builder)).Build();
         this.componentsByType = components.ToDictionary(c => c.GetType());
      }

      public BootstrapConfiguration GetBootstrapConfiguration() { return this.bootstrapConfiguration; }

      public TConfigurationComponent GetComponentOrNull<TConfigurationComponent>()
         where TConfigurationComponent : IConfigurationComponent { return (TConfigurationComponent)componentsByType.GetValueOrDefault(typeof(TConfigurationComponent)); }
   }

   public class InjectedModuleConfigurationBuilder
   {
      private readonly List<IConfigurationComponent> components = new List<IConfigurationComponent>();

      public void AddComponent(IConfigurationComponent component) { components.Add(component); }

      public InjectedModuleConfiguration Build() { return new InjectedModuleConfiguration(components); }
   }
}
