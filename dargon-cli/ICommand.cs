namespace Dargon.CLI {
   public interface ICommand {
      string Name { get; }
      int Eval(string subcommand);
   }
}