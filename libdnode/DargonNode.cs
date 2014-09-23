using ItzWarty;
using System;
using System.Collections.Generic;

namespace Dargon.IO
{
   public class DargonNode : IWritableDargonNode
   {
      private string name;
      private IWritableDargonNode parent;
      private readonly List<IWritableDargonNode> children = new List<IWritableDargonNode>();
      private readonly Dictionary<Type, object> components = new Dictionary<Type, object>(); 

      public DargonNode(string name = null) { this.name = null; }

      public string Name { get { return name; } set { name = value; } }

      IReadableDargonNode IReadableDargonNode.Parent { get { return parent; } }
      public IWritableDargonNode Parent { get { return parent; } set { SetParent(value); } }
      
      IReadOnlyList<IReadableDargonNode> IReadableDargonNode.Children { get { return children; } }
      public IReadOnlyList<IWritableDargonNode> Children { get { return children; } }

      public void AddComponent(Type componentInterface, object component) { components.Add(componentInterface, component); }
      public T GetComponentOrNull<T>() { return (T)components.GetValueOrDefault(typeof(T)); }

      public bool AddChild(IWritableDargonNode node)
      {
         if (!this.children.Contains(node)) {
            this.children.Add(node);
            node.Parent = this;
         }
         return true;
      }

      public bool RemoveChild(IWritableDargonNode node)
      {
         if (this.children.Contains(node))
         {
            this.children.Remove(node);
            node.Parent = null;
         }
         return true;
      }

      private void SetParent(IWritableDargonNode value)
      {
         if (value != this.parent) {
            var oldParent = this.parent;
            if (oldParent != null) {
               this.parent = null;
               oldParent.RemoveChild(this);
            }

            if (value != null) {
               value.AddChild(this);
            }
            this.parent = value;
         }
      }
   }
}
