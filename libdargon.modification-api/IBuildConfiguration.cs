using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dargon.Modifications
{
   public interface IBuildConfiguration
   {
      [JsonProperty("per_target_build", Required = Required.Default)]
      [DefaultValue(true)]
      bool PerTargetBuild { get; }
   }
}
