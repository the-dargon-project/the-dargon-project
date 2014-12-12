namespace Dargon.InjectedModule.Commands
{
   public interface ICommand
   {
      string Type { get; }
      byte[] Data { get; }
   }
}