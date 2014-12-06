namespace Dargon.Wyvern.Specialized {
   public interface IDistributedCounter {
      long PeekCurrentValue();
      long TakeNextValue();
   }
}
