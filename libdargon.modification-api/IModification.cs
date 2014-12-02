using System;
using Dargon.Game;
using Dargon.PortableObjects;

namespace Dargon.Modifications
{
   public interface IModification : IPortableObject
   {
      string RepositoryName { get; }
      string RepositoryPath { get; }

      IModificationMetadata Metadata { get; }
      IBuildConfiguration BuildConfiguration { get; }
   }
}
