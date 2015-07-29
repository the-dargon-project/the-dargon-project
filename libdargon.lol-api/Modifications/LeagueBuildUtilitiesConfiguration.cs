using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.LeagueOfLegends.Modifications {
   public class LeagueBuildUtilitiesConfiguration {
      private const string kEnableBuildLoggingKey = "dargon.leagueoflegends.modifications.enable_build_logging";
      private readonly SystemState systemState;
      private readonly bool isLoggingEnabled;

      public LeagueBuildUtilitiesConfiguration(SystemState systemState) {
         this.systemState = systemState;
         this.isLoggingEnabled = systemState.GetBoolean(kEnableBuildLoggingKey, false);
      }

      public bool IsLoggingEnabled => isLoggingEnabled;
   }
}
