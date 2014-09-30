using System.Collections.Generic;
using ItzWarty.Collections;

namespace Dargon.InjectedModule.Components
{
   public class RoleConfigurationComponent : IConfigurationComponent
   {
      private const string kRolePropertyName = "role";

      private static readonly IReadOnlyDictionary<DimRole, string> valueByRole = ImmutableDictionary.Of(
         DimRole.Launcher, "LAUNCHER",
         DimRole.Client, "CLIENT",
         DimRole.Game, "GAME"
      );

      private readonly DimRole role;
      private readonly string value;

      public RoleConfigurationComponent(DimRole role)
      {
         this.role = role;
         value = valueByRole[role];
      }

      public DimRole Role { get { return role; } }
      public string Value { get { return value; } }

      public void AmendBootstrapConfiguration(BootstrapConfigurationBuilder builder) { builder.SetProperty(kRolePropertyName, Value); }
   }
}
