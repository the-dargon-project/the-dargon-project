using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.IO;
using Dargon.IO.Drive;
using ItzWarty;
using ItzWarty.IO;

namespace Dargon.Client.ViewModels {
   public class ModificationImportViewModelFactory {
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly IDriveNodeFactory driveNodeFactory;

      public ModificationImportViewModelFactory(IFileSystemProxy fileSystemProxy, IDriveNodeFactory driveNodeFactory) {
         this.fileSystemProxy = fileSystemProxy;
         this.driveNodeFactory = driveNodeFactory;
      }

      public ModificationImportEntryViewModelBase FromDirectory(string basePath) {
         var modificationRootNode = driveNodeFactory.CreateFromDirectory(basePath);
         var modificationRootFileInfo = fileSystemProxy.GetFileInfo(modificationRootNode.GetPath());
         if (!modificationRootFileInfo.Attributes.HasFlag(FileAttributes.Directory)) {
            throw new InvalidOperationException("basePath must be a directory!");
         }
         var viewModelRoot = new ModificationImportEntryDirectoryViewModel { Name = basePath };
         FromDirectoryHelper(modificationRootNode, viewModelRoot);
         return viewModelRoot;
      }

      private void FromDirectoryHelper(IWritableDargonNode fileNode, ModificationImportEntryDirectoryViewModel viewModelNode) {
         foreach (var fileNodeChild in fileNode.Children) {
            if (fileNodeChild.Children.None()) {
               viewModelNode.Children.Add(new ModificationImportEntryFileViewModel { Name = fileNodeChild.Name, Parent = viewModelNode });
            } else {
               var newDirectoryName = fileNodeChild.Name;
               var currentNode = fileNodeChild;
               while (currentNode.Children.Count == 1 && currentNode.Children.First().Children.Count != 0) {
                  currentNode = currentNode.Children.First();
                  newDirectoryName = newDirectoryName + "/" + currentNode.Name;
               }
               var directoryNode = new ModificationImportEntryDirectoryViewModel { Name = newDirectoryName, Parent = viewModelNode }; 
               viewModelNode.Children.Add(directoryNode);
               FromDirectoryHelper(currentNode, directoryNode);
            }
         }
      }
   }
}
