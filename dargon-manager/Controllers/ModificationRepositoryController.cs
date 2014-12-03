using System;
using System.Collections.Generic;
using System.Linq;
using Dargon.Manager.Models;
using Dargon.Manager.ViewModels;
using Dargon.ModificationRepositories;
using ItzWarty;

namespace Dargon.Manager.Controllers {
   public class ModificationRepositoryController {
      private readonly ModificationRepositoryService modificationRepositoryService;
      private readonly LocalRepositoryModificationList localRepositoryModificationList;

      public ModificationRepositoryController(ModificationRepositoryService modificationRepositoryService, ModificationListingViewModel modificationListingViewModel) {
         this.modificationRepositoryService = modificationRepositoryService;
         this.localRepositoryModificationList = new LocalRepositoryModificationList(modificationRepositoryService);
      }

      public SynchronizedModificationList GetSynchronizedLocalRepositoryModifications() {
         return localRepositoryModificationList;
      }

      public sealed class LocalRepositoryModificationList : SynchronizedModificationList {
         private readonly ModificationRepositoryService modificationRepositoryService;

         public LocalRepositoryModificationList(ModificationRepositoryService modificationRepositoryService) {
            this.modificationRepositoryService = modificationRepositoryService;

            Fetch();
         }

         protected override IReadOnlyList<ModificationViewModel> FetchInternal() {
            var mods = modificationRepositoryService.EnumerateModifications().ToArray();
            var results = Util.Generate(mods.Length, i => new ModificationViewModel(mods[i]));
            Console.WriteLine("Fetched " + mods.Length + " modifications");
            return results;
         }
      }
   }
}