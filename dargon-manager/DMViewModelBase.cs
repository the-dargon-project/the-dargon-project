using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DargonManager;
using DargonManager.Annotations;

namespace Dargon.Manager
{
   /// <summary>
   /// Handles Dargon Manager's sidebar tagging and modification display.  The Dargon Manager View
   /// model filters through Dargon's modifications and their changesets.
   /// </summary>
   public abstract class DMViewModelBase : INotifyPropertyChanged
   {
      ///// <summary>
      ///// The Dargon Service... which might be remoted.
      ///// </summary>
      //public IDargonService Dargon { get; private set; }

      ///// <summary>
      ///// The categories of our Dargon Manager View Model.
      ///// </summary>
      //public ObservableCollection<DMCategory> Categories { get { return m_categories; } }
      //private readonly ObservableCollection<DMCategory> m_categories = new ObservableCollection<DMCategory>();

      ///// <summary>
      ///// The category currently selected and displayed by Dargon Manager.
      ///// </summary>
      //public DMCategory SelectedCategory 
      //{ 
      //   get { return m_selectedCategory; }
      //   set { SelectCategory(value); }
      //}
      //private DMCategory m_selectedCategory;

      /// <summary>
      /// The entities that are being displayed in our entity listing.
      /// </summary>
      public ObservableCollection<object> ListedEntities { get { return m_listedEntities; } }
      private readonly ObservableCollection<object> m_listedEntities = new ObservableCollection<object>();

      public string Status { get { return m_status ?? kDefaultStatus; } set { m_status = value; OnPropertyChanged(); } }
      private string m_status;
      private const string kDefaultStatus = "Your modification library is listed above.\r\nTo import modifications, Drag and Drop them into this window.";

      public DMModificationImportStatus ModificationImportStatus { get { return m_importStatus; } set { m_importStatus = value; OnPropertyChanged(); } }
      private DMModificationImportStatus m_importStatus = DMModificationImportStatus.None;

      public abstract void ImportModifications(string[] drop);

      /*
      /// <summary>
      /// Initializes a new instance of a Dargon Manager view model.
      /// </summary>
      /// <param name="dargon"></param>
      /// <param name="gameType"></param>
      public DMViewModel(IDargonService dargon)
      {
         Dargon = dargon;

         // Create default categories
         AddCategory("All", DMCategoryEntityType.Modification, (tags) => true);
         AddCategory("Starred", DMCategoryEntityType.Modification, (tags) => tags.Contains(ModificationTag.Starred));

         AddCategory("Legacy", DMCategoryEntityType.ChangeSet, (tags) => tags.Contains((ModificationTag)LolModificationTag.Legacy));
         AddCategory("Actors", DMCategoryEntityType.ChangeSet, (tags) => tags.Contains((ModificationTag)LolModificationTag.Actors));
         AddCategory("Maps", DMCategoryEntityType.ChangeSet, (tags) => tags.Contains((ModificationTag)LolModificationTag.Maps));
         AddCategory("User Interface", DMCategoryEntityType.ChangeSet, (tags) => tags.Contains((ModificationTag)LolModificationTag.UserInterface));
         AddCategory("Runtime", DMCategoryEntityType.ChangeSet, (tags) => tags.Contains((ModificationTag)LolModificationTag.Runtime));

         // Subscribe to modification events.  TODO: Support for other games.
         var lol = dargon.LeagueOfLegends;
         var mods = lol.Modifications; //TODO: mods.Added and removed

         mods.Added += HandleModificationAdded;
         mods.Removed += HandleModificationRemoved;
         foreach (var mod in mods)
            HandleModificationAdded(this, new ModificationEventArgs(mod));

         // Select default category
         SelectCategory("All");
      }

      /// <summary>
      /// Initializes a new instance of a Dargon Manager view model.
      /// This view model isn't bound to a dargon instance at all, so it basically represents dummy data
      /// </summary>
      public DMViewModel(List<SampleModification> sampleMods)
      {
         foreach (var mod in sampleMods)
            m_listedEntities.Add(mod);
      }


      /// <summary>
      /// Adds a new category to the Dargon Manager view model
      /// </summary>
      /// <param name="name"></param>
      /// <param name="entityType"></param>
      /// <param name="condition"></param>
      public void AddCategory(string name, DMCategoryEntityType entityType, DMIsCategoryMatch condition)
      {
         m_categories.Add(new DMCategory(name, entityType, condition));
      }

      /// <summary>
      /// Selects the category of the given name
      /// </summary>
      /// <param name="name"></param>
      public void SelectCategory(string name)
      {
         foreach (var category in m_categories)
         {
            if (category.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
               SelectCategory(category);
               return;
            }
         }
         throw new KeyNotFoundException("Couldn't find category of name " + name);
      }

      /// <summary>
      /// Selects the given category for display in the Entity Listing.
      /// </summary>
      /// <param name="category"></param>
      public void SelectCategory(DMCategory category)
      {
         if (m_selectedCategory != null)
            m_selectedCategory.NewEntities.Clear();
         m_selectedCategory = category;
         m_listedEntities.Clear();

         Console.WriteLine("Select Category " + category.Name + " " + category.EntityType);
         var lol = Dargon.LeagueOfLegends;
         foreach (var mod in lol.Modifications)
         {
            Console.WriteLine("MOD " + mod.Name + " ");
            if (category.EntityType == DMCategoryEntityType.Modification)
            {
               if (category.IsCandidateMatch(mod, mod.Tags))
               {
                  m_listedEntities.Add(new DMModificationViewModel(mod));
               }
            }
            else if (category.EntityType == DMCategoryEntityType.ChangeSet)
            {
               foreach (var changeset in mod.ChangeSets)
               {
                  if (category.IsCandidateMatch(changeset, changeset.Tags))
                     m_listedEntities.Add(new DMChangeSetViewModel(changeset)); // TODO: Changeset viewmodel
               }
            }
         }
      }

      //-------------------------------------------------------------------------------------------
      // Subscription Handlers for when modifications are added or removed from our collection.
      //     - Subscribes to member events of the modification and its changesets.
      //-------------------------------------------------------------------------------------------
      private void HandleModificationAdded(object sender, ModificationEventArgs e)
      {
         e.Modification.ChangeSets.Added += HandleChangeSetAdded;
         e.Modification.ChangeSets.Removed += HandleChangeSetRemoved;

         e.Modification.Tags.Added += HandleModificationTagsAdded;
         e.Modification.Tags.Removed += HandleModificationTagsRemoved;

         // Tell categories that the mod and its changesets have been added
         foreach (var category in m_categories)
         {
            if (category.EntityType == DMCategoryEntityType.Modification)
            {
               category.HandleCandidateEntity(e.Modification, e.Modification.Tags);
            }
            else if(category.EntityType == DMCategoryEntityType.ChangeSet)
            {
               foreach (var changeSet in e.Modification.ChangeSets)
               {
                  category.HandleCandidateEntity(changeSet, changeSet.Tags);
               }
            }
         }
      }

      private void HandleModificationRemoved(object sender, ModificationEventArgs e)
      {
         // Remove the modification from our list of all modifications
         e.Modification.Tags.Removed -= HandleModificationTagsRemoved;
         e.Modification.Tags.Added -= HandleModificationTagsAdded;

         e.Modification.ChangeSets.Removed -= HandleChangeSetRemoved;
         e.Modification.ChangeSets.Added -= HandleChangeSetAdded;

         // TODO: Tell categories that a mod has been removed
      }

      //-------------------------------------------------------------------------------------------
      // Subscription Handlers for when tags are added or removed from our modifications.  Used
      // to update our view-model.
      //-------------------------------------------------------------------------------------------
      private void HandleModificationTagsAdded(object sender, ObservableSetEventArgs<ModificationTag> e)
      {
         var mod = (IModification)((TagCollection<ModificationTag>)e.Set).Owner;
         foreach (var category in Categories)
         {
            if (category.EntityType == DMCategoryEntityType.Modification)
            {
               category.HandleCandidateEntity(mod, mod.Tags);
            }
         }

         // Tell categories that the mod has had its tags changed
         foreach (var category in m_categories)
         {
            if (category.EntityType == DMCategoryEntityType.Modification)
            {
               category.HandleCandidateEntity(mod, mod.Tags);
            }
         }
      }

      private void HandleModificationTagsRemoved(object sender, ObservableSetEventArgs<ModificationTag> e)
      {
         var mod = (IModification)((TagCollection<ModificationTag>)e.Set).Owner;
         foreach (var category in Categories)
         {
            if (category.EntityType == DMCategoryEntityType.Modification)
            {
               category.HandleCandidateEntity(mod, mod.Tags);
            }
         }

         // Tell categories that the mod has had its tags changed
         foreach (var category in m_categories)
         {
            if (category.EntityType == DMCategoryEntityType.Modification)
            {
               category.HandleCandidateEntity(mod, mod.Tags);
            }
         }
      }

      //-------------------------------------------------------------------------------------------
      // Subscription Handlers for when changesets are are added or removed from our modification.
      //     - Subscribes to member events of the changesets.
      //-------------------------------------------------------------------------------------------
      private void HandleChangeSetAdded(ObservableSet<IChangeSet> sender, ObservableSetEventArgs<IChangeSet> e)
      {
         e.Element.Tags.Added += HandleChangeSetTagsAdded;
         e.Element.Tags.Removed += HandleChangeSetTagsRemoved;

         // Tell categories that a changeset has been added
         foreach (var category in m_categories)
         {
            if (category.EntityType == DMCategoryEntityType.ChangeSet)
            {
               category.HandleCandidateEntity(e.Element, e.Element.Tags);
            }
         }
      }

      private void HandleChangeSetRemoved(ObservableSet<IChangeSet> sender, ObservableSetEventArgs<IChangeSet> e)
      {
         e.Element.Tags.Removed -= HandleChangeSetTagsRemoved;
         e.Element.Tags.Added -= HandleChangeSetTagsAdded;

         // TODO: Tell categories that a changeset has been removed
      }

      //-------------------------------------------------------------------------------------------
      // Subscription Handlers for when tags are added or removed from our changesets.  Used to
      // update our view-model.
      //-------------------------------------------------------------------------------------------
      private void HandleChangeSetTagsAdded(object sender, ObservableSetEventArgs<ModificationTag> e)
      {
         var changeSet = (IChangeSet)(((TagCollection<ModificationTag>)sender).Owner);
         foreach (var category in Categories)
         {
            if (category.EntityType == DMCategoryEntityType.ChangeSet)
            {
               category.HandleCandidateEntity(changeSet, changeSet.Tags);
            }
         }

         // Tell categories that a changeset has had its tags changed
         foreach (var category in m_categories)
         {
            if (category.EntityType == DMCategoryEntityType.ChangeSet)
            {
               category.HandleCandidateEntity(changeSet, changeSet.Tags);
            }
         }
      }

      private void HandleChangeSetTagsRemoved(object sender, ObservableSetEventArgs<ModificationTag> e)
      {
         var changeSet = (IChangeSet)(((TagCollection<ModificationTag>)sender).Owner);
         foreach (var category in Categories)
         {
            if (category.EntityType == DMCategoryEntityType.ChangeSet)
            {
               category.HandleCandidateEntity(changeSet, changeSet.Tags);
            }
         }

         // Tell categories that a changeset has had its tags changed
         foreach (var category in m_categories)
         {
            if (category.EntityType == DMCategoryEntityType.ChangeSet)
            {
               category.HandleCandidateEntity(changeSet, changeSet.Tags);
            }
         }
      }
       */
      public event PropertyChangedEventHandler PropertyChanged;

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         PropertyChangedEventHandler handler = PropertyChanged;
         if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}
