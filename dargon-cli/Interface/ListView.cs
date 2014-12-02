using System;
using System.Collections.Generic;
using System.Linq;
using ItzWarty;

namespace Dargon.CLI.Interface {
   public class ListView {
      private readonly IReadOnlyList<string> model;

      public ListView(IEnumerable<string> model) {
         this.model = model.ToArray();
      }

      public void PrintToConsole() {
         var maxLength = model.Max(s => s.Length);
         var paddingString = "  ";
         var charsPerLine = Console.BufferWidth;
         var wordsPerLine = charsPerLine / (maxLength + paddingString.Length);
         var rowCount = (model.Count + wordsPerLine - 1) / wordsPerLine;
         for (var row = 0; row < rowCount; row++) {
            for (var col = 0; col < wordsPerLine; col++) {
               var i = col * rowCount + row;
               if (i < model.Count) {
                  Console.Write(model[i].PadRight(maxLength) + (col + 1 == wordsPerLine ? "" : paddingString));
               }
            }
            Console.WriteLine();
         }
      }
   }
}
