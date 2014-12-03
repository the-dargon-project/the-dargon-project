namespace Dargon {
   public interface SystemState {
      string Get(string key, string defaultValue);
      void Set(string key, string value);
   }
}
