using System;
using Dargon.Game;

namespace Dargon.Modifications
{
   public interface IModification
   {
      string RepositoryName { get; }
      string RepositoryPath { get; }

      IModificationMetadata Metadata { get; }
      IBuildConfiguration BuildConfiguration { get; }
   }
}
