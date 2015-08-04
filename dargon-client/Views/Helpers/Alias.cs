using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;

namespace Dargon.Client.Views.Helpers {
   public class Alias : MarkupExtension {
      public string ResourceKey { get; set; }

      public Alias() {}

      public Alias(string resourceKey) {
         ResourceKey = resourceKey;
      }

      public override object ProvideValue(IServiceProvider serviceProvider) {
         return _ProvideLocalValue(serviceProvider) ?? _ProvideApplicationValue();
      }

      private object _ProvideLocalValue(IServiceProvider serviceProvider) {
         var rootObjectProvider = (IRootObjectProvider)
            serviceProvider.GetService(typeof(IRootObjectProvider));
         if (rootObjectProvider == null) return null;
         var dictionary = rootObjectProvider.RootObject as IDictionary;
         if (dictionary == null) return null;
         return dictionary.Contains(ResourceKey) ? dictionary[ResourceKey] : null;
      }

      private object _ProvideApplicationValue() {
         var value = Application.Current.TryFindResource(ResourceKey);
         return value;
      }
   }
}
