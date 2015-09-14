namespace Dargon {
   public class PrimaryFallbackSystemStateImpl : SystemState {
      private readonly SystemState primary;
      private readonly SystemState fallback;

      public PrimaryFallbackSystemStateImpl(SystemState primary, SystemState fallback) {
         this.primary = primary;
         this.fallback = fallback;
      }

      public string Get(string key, string defaultValue) {
         string value;
         if (!TryGet(key, out value)) {
            value = defaultValue;
         }
         return value;
      }

      public bool TryGet(string key, out string value) => primary.TryGet(key, out value) || fallback.TryGet(key, out value);

      public void Set(string key, string value) => primary.Set(key, value);

      public bool Get(string key, bool defaultValue) {
         bool value;
         if (!TryGet(key, out value)) {
            value = defaultValue;
         }
         return value;
      }

      public bool TryGet(string key, out bool value) => primary.TryGet(key, out value) || fallback.TryGet(key, out value);

      public void Set(string key, bool value) => primary.Set(key, value);

      public int Get(string key, int defaultValue) {
         int value;
         if (!TryGet(key, out value)) {
            value = defaultValue;
         }
         return value;
      }

      public bool TryGet(string key, out int value) => primary.TryGet(key, out value) || fallback.TryGet(key, out value);

      public void Set(string key, int value) => primary.Set(key, value);
   }
}