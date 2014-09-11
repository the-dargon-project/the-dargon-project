using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.IO
{
   public static class Extensions
   {
      public static IReadableDargonNode GetRoot(this IReadableDargonNode node)
      {
         while (node.Parent != null) {
            node = node.Parent;
         }
         return node;
      }

      public static IWritableDargonNode GetRoot(this IWritableDargonNode node)
      {
         while (node.Parent != null) {
            node = node.Parent;
         }
         return node;
      }

      public static IReadableDargonNode GetChild(this IReadableDargonNode node, string name) { return node.Children.First((child) => child.Name.Equals(name, StringComparison.OrdinalIgnoreCase)); }
      public static IWritableDargonNode GetChild(this IWritableDargonNode node, string name) { return node.Children.First((child) => child.Name.Equals(name, StringComparison.OrdinalIgnoreCase)); }

      public static IReadableDargonNode GetChildOrNull(this IReadableDargonNode node, string name) { return node.Children.FirstOrDefault((child) => child.Name.Equals(name, StringComparison.OrdinalIgnoreCase)); }
      public static IWritableDargonNode GetChildOrNull(this IWritableDargonNode node, string name) { return node.Children.FirstOrDefault((child) => child.Name.Equals(name, StringComparison.OrdinalIgnoreCase)); }

      public static bool TryGetChild(this IReadableDargonNode node, string name, out IReadableDargonNode result)
      {
         foreach (var child in node.Children)
         {
            if (child.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
               result = child;
               return true;
            }
         }
         result = null;
         return false;
      }

      public static bool TryGetChild(this IWritableDargonNode node, string name, out IWritableDargonNode result)
      {
         foreach (var child in node.Children)
         {
            if (child.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
               result = child;
               return true;
            }
         }
         result = null;
         return false;
      }

      public static string GetPath(this IReadableDargonNode node, string delimiter = "/")
      {
         var s = new Stack<string>();
         s.Push(node.Name);
         node = node.Parent;
         while (node != null) {
            s.Push(delimiter);
            s.Push(node.Name);
            node = node.Parent;
         }
         var sb = new StringBuilder();
         while (s.Any()) {
            sb.Append(s.Pop());
         }
         return sb.ToString();
      }
   }
}
