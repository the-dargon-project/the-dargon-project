using System.Collections.Generic;

namespace Dargon.CLI {
   public interface ICommand {
      string Name { get; }

      int Eval(string subcommand);
   }

   public interface IDispatcher {
      string Name { get; }
      IReadOnlyDictionary<string, ICommand> CommandsByName { get; }

      int Eval(string input);

      IDispatcher Parent { get; set; }
   }
}