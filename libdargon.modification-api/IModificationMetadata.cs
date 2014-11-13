using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dargon.Game;
using Newtonsoft.Json;

namespace Dargon.Modifications
{
   public interface IModificationMetadata
   {
      [JsonProperty("name", Required = Required.Default)]
      [DefaultValue("")]
      string Name { get; }

      [JsonProperty("authors", Required = Required.Always)]
      string[] Authors { get; }

      [JsonProperty("version", Required = Required.Default)]
      [DefaultValue("")]
      string Version { get; }

      [JsonProperty("targets", Required = Required.Always)]
      GameType[] Targets { get; }

      [JsonProperty("website", Required = Required.Default)]
      [DefaultValue("")]
      string Website { get; }

      [JsonProperty("toggle_url", Required = Required.Default)]
      [DefaultValue("")]
      string ToggleUrl { get; }

      [JsonProperty("content_path", Required = Required.Default)]
      [DefaultValue("content")]
      string ContentPath { get; }
   }
}
