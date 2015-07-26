using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dargon.Client.Annotations;
using ItzWarty;

namespace Dargon.Client.ViewModels {
   public enum ResolutionState {
      Resolving            = 0,
      ResolutionSuccessful = 1,
      ResolutionFailed     = 2
   }

   public abstract class ModificationImportEntryViewModelBase : INotifyPropertyChanged {
      private string name;
      private ModificationImportEntryDirectoryViewModel parent;
      private int unresolvedDescendentsCount;
      private int resolvedDescendentsCount;
      private ResolutionState resolutionState;
      private string resolutionPath;
      public event PropertyChangedEventHandler PropertyChanged;

      public string Name { get { return name; } set { name = value; OnPropertyChanged(); } }
      public ModificationImportEntryDirectoryViewModel Parent { get { return parent; } set { parent = value; OnPropertyChanged(); } }
      public ObservableCollection<ModificationImportEntryViewModelBase> Children { get; set; } = new ObservableCollection<ModificationImportEntryViewModelBase>();
      public int UnresolvedDescendentsCount { get { return unresolvedDescendentsCount; } set { unresolvedDescendentsCount = value; OnPropertyChanged(); } }
      public int ResolvedDescendentsCount { get { return resolvedDescendentsCount; } set { resolvedDescendentsCount = value; OnPropertyChanged(); } }
      public string ResolutionPath { get { return resolutionPath; } set { resolutionPath = value; OnPropertyChanged(); } }
      public ResolutionState ResolutionState { get { return resolutionState; } set { SetResolutionState(value); } }
      public abstract bool IsDirectory { get; }

      public string Path => ComputePathInternal();


      private string ComputePathInternal() {
         var sb = new StringBuilder();
         sb.Append(Name);
         var currentNode = this.Parent;
         while (currentNode != null) {
            sb.Insert(0, '/');
            sb.Insert(0, currentNode.Name);
            currentNode = currentNode.Parent;
         }
         return sb.ToString();
      }

      private void SetResolutionState(ResolutionState value) {
         if (resolutionState != value) {
            if (resolutionState == ResolutionState.ResolutionSuccessful) {
               var currentNode = this.Parent;
               while (currentNode != null) {
                  currentNode.ResolvedDescendentsCount--;
                  currentNode = currentNode.Parent;
               }
            } else if (resolutionState == ResolutionState.ResolutionFailed) {
               var currentNode = this.Parent;
               while (currentNode != null) {
                  currentNode.UnresolvedDescendentsCount--;
                  currentNode = currentNode.Parent;
               }
            }

            if (value == ResolutionState.ResolutionSuccessful) {
               var currentNode = this.Parent;
               while (currentNode != null) {
                  currentNode.ResolvedDescendentsCount++;
                  currentNode = currentNode.Parent;
               }
            } else if (value == ResolutionState.ResolutionFailed) {
               var currentNode = this.Parent;
               while (currentNode != null) {
                  currentNode.UnresolvedDescendentsCount++;
                  currentNode = currentNode.Parent;
               }
            }

            this.resolutionState = value;
            OnPropertyChanged(nameof(ResolutionState));
         }
      }

      public IEnumerable<ModificationImportEntryViewModelBase> EnumerateFileNodes() {
         var s = new Stack<ModificationImportEntryViewModelBase>();
         s.Push(this);
         while (s.Any()) {
            var node = s.Pop();
            if (!node.IsDirectory) {
               yield return node;
            }
            node.Children.Reverse().ForEach(s.Push);
         }
      }
      
      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}
