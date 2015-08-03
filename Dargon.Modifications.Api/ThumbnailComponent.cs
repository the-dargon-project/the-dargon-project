using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;
using ItzWarty;
using NLog;

namespace Dargon.Modifications {
   [ModificationComponent(ComponentOrigin.Local, "DC_SELECTED_THUMBNAIL")]
   public class ThumbnailComponent : Component {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      private const int kVersion = 0;
      private readonly Modification modification;
      private string selectedThumbnailPath;
      public event PropertyChangedEventHandler PropertyChanged;

      public ThumbnailComponent() {
//         throw new InvalidOperationException("Thumbnail Component should not be default constructed!");
      }

      public ThumbnailComponent(Modification modification) {
         this.modification = modification;
      }

      public string SelectedThumbnailPath { get { return selectedThumbnailPath; } set { selectedThumbnailPath = value; OnPropertyChanged(); } }

      public void SelectThumbnailIfUnselected() {
         if (SelectedThumbnailPath != null) {
            logger.Info($"Modification '{modification.RepositoryName}' already has selected thumbnail!");
         } else {
            logger.Info($"Selected thumbnail for modification '{modification.RepositoryName}' was null!");
            var thumbnailsDirectory = new DirectoryInfo(Path.Combine(modification.RepositoryPath, "thumbnails"));

            if (!thumbnailsDirectory.Exists) {
               logger.Info($"Thumbnail directory doesn't exist!");
            } else {
               logger.Info($"Thumbnail directory exists!");
               var thumbnailCandidates = thumbnailsDirectory.EnumerateFiles("*", SearchOption.AllDirectories);
               var selectedThumbnail = thumbnailCandidates.Where(x => !Util.IsThrown<Exception>(() => Image.FromFile(x.FullName))).FirstOrDefault();
               if (selectedThumbnail == null) {
                  logger.Info($"Failed to find loadable thumbnail image file!");
               } else {
                  logger.Info($"Selected thumbnail path {selectedThumbnail.FullName}!");
                  SelectedThumbnailPath = selectedThumbnail.FullName;
               }
            }
         }
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteS32(0, kVersion);
         writer.WriteString(1, SelectedThumbnailPath);
      }

      public void Deserialize(IPofReader reader) {
         var version = reader.ReadS32(0);
         SelectedThumbnailPath = reader.ReadString(1);
         Trace.Assert(version == kVersion, "version == kVersion");
      }

      public void Load(Component component) {
         var source = (ThumbnailComponent)component;
         SelectedThumbnailPath = source.SelectedThumbnailPath;
      }

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         Console.WriteLine("TC : " + propertyName);
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}
