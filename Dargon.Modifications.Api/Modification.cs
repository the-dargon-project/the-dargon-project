namespace Dargon.Modifications {
   public interface Modification {
      string RepositoryName { get; }
      string RepositoryPath { get; }
      string MetadataPath { get; }
      TComponent GetComponent<TComponent>() where TComponent : Component, new();
   }
}