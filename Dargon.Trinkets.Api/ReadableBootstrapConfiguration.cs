using ItzWarty.Collections;
using System.Collections.Generic;

namespace Dargon.Trinkets {
   public interface ReadableBootstrapConfiguration {
      IReadOnlySet<string> Flags { get; }
      IDictionary<string, string> Properties { get; }
   }
}