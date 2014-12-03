using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dargon.Modifications;
using ItzWarty;

namespace Dargon.Manager.Models {
   public abstract class SynchronizedModificationList : ObservableCollection<ModificationViewModel> {
      public void Fetch() {
         var oldValues = new HashSet<ModificationViewModel>(this);
         var fetchedValues = new HashSet<ModificationViewModel>(FetchInternal());
         var newValues = new HashSet<ModificationViewModel>();

         foreach (var oldValue in oldValues.Where(ov => !fetchedValues.Any(cv => cv.RepositoryName.Equals(ov.RepositoryName)))) {
            this.Remove(oldValue);
         }
         foreach (var newValue in fetchedValues.Where(fv => !oldValues.Any(ov => ov.RepositoryName.Equals(fv.RepositoryName)))) {
            this.Add(newValue);
            newValues.Add(newValue);
         }
         foreach (var mod in this.Where(v => !newValues.Contains(v))) {
            var match = fetchedValues.First(fv => fv.RepositoryName.Equals(mod.RepositoryName));
            mod.Update(match);
         }
         Console.WriteLine("Fetch result: " + oldValues.Count + " removed " + newValues.Count + " added " + (this.Count - newValues.Count) + " kept.");
      }

      protected abstract IReadOnlyList<ModificationViewModel> FetchInternal();
   }
}
