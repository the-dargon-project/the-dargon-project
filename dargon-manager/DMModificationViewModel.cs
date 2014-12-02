using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Dargon.Modifications;
using DargonManager;
using DargonManager.Annotations;

namespace Dargon.Manager
{
   public class DMModificationViewModel : IDMListingViewModel
   {
      /// <summary>
      /// The Dargon Modification represented by the Modification View Model.
      /// </summary>
      private readonly IModification m_mod;

      /// <summary>
      /// Initializes a new instance of a Dargon Manager modification view-model.
      /// </summary>
      /// <param name="mod"></param>
      public DMModificationViewModel(IModification mod)
      {
         m_mod = mod;
      }

      /// <summary>
      /// The name of our modification
      /// </summary>
      public string Name 
      {
         get { return m_mod.RepositoryName; }
         set { throw new InvalidOperationException(); OnPropertyChanged("Name"); }
      }

      /// <summary>
      /// The name of our modification
      /// </summary>
      public string Author
      {
         get { return string.Join(", ", m_mod.Metadata.Authors); }
         set { throw new InvalidOperationException(); OnPropertyChanged("Author"); }
      }

      /// <summary>
      /// Whether or not our modification's changesets are enabled.
      /// </summary>
      public bool IsEnabled
      {
         get { return true; }
         set { throw new NotImplementedException(); OnPropertyChanged("IsEnabled"); }
      }

      //-------------------------------------------------------------------------------------------
      // Boilerplate Code
      //-------------------------------------------------------------------------------------------
      public event PropertyChangedEventHandler PropertyChanged;
      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         PropertyChangedEventHandler handler = PropertyChanged;
         if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
      }


      private string m_savedName, m_savedAuthor;
      private bool m_savedIsEnabled;
      /// <summary>
      /// Begins an edit on an object.
      /// </summary>
      public void BeginEdit()
      {
         m_savedName = Name;
         m_savedAuthor = Author;
         m_savedIsEnabled = IsEnabled;
      }

      /// <summary>
      /// Pushes changes since the last <see cref="M:System.ComponentModel.IEditableObject.BeginEdit"/> or <see cref="M:System.ComponentModel.IBindingList.AddNew"/> call into the underlying object.
      /// </summary>
      public void EndEdit()
      {
      }

      /// <summary>
      /// Discards changes since the last <see cref="M:System.ComponentModel.IEditableObject.BeginEdit"/> call.
      /// </summary>
      public void CancelEdit()
      {
         Name = m_savedName;
         Author = m_savedAuthor;
         IsEnabled = m_savedIsEnabled;
      }
   }
}
