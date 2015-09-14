using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon {
   public abstract class SystemStateImplBase : SystemState {
      public string Get(string key, string defaultValue) {
         string result;
         if (!TryGet(key, out result)) {
            result = defaultValue;
         }
         return result;
      }

      public abstract bool TryGet(string key, out string value);
      public abstract void Set(string key, string value);

      public bool Get(string key, bool defaultValue) {
         bool result;
         if (!TryGet(key, out result)) {
            result = defaultValue;
         }
         return result;
      }

      public bool TryGet(string key, out bool value) {
         string valueString;
         if (TryGet(key, out valueString) &&
             bool.TryParse(valueString, out value)) {
            return true;
         } else {
            value = false;
            return false;
         }
      }

      public void Set(string key, bool value) {
         Set(key, value.ToString());
      }

      public int Get(string key, int defaultValue) {
         int result;
         if (!TryGet(key, out result)) {
            result = defaultValue;
         }
         return result;
      }

      public bool TryGet(string key, out int value) {
         string valueString;
         if (TryGet(key, out valueString) &&
             int.TryParse(valueString, out value)) {
            return true;
         } else {
            value = 0;
            return false;
         }
      }

      public void Set(string key, int value) {
         Set(key, value.ToString());
      }
   }
}
