using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Dargon.Manager.Models;

namespace Dargon.Manager.Controllers {
   public class ModificationImportController {
      private readonly StatusController statusController;
      private readonly ImportValidityModelImpl importValidityModel;

      public ModificationImportController(StatusController statusController, ImportValidityModelImpl importValidityModel) {
         this.statusController = statusController;
         this.importValidityModel = importValidityModel;
      }

      public bool ValidateDropHover(string[] paths) {
         if (paths.Length != 1) {
            statusController.Update("Invalid Drag/Drop! \r\n Only *.zip, *.rar, a directory, or an individual file may be dropped in at this time.");
            importValidityModel.Validity = ImportValidity.Invalid;
            return false;
         } else {
            string name = new FileInfo(paths[0]).Name;
            if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {
               name = name.Substring(name.Length - 4);
            }

            statusController.Update("Modification will be imported as " + name + "\r\nRelease your mouse button to finish importing the modification.");
            importValidityModel.Validity = ImportValidity.Valid;
            return true;
         }
      }

      public void HandleDragLeave() {
         statusController.Update(""); 
         importValidityModel.Validity = ImportValidity.Undefined;
      }
   }
}
