namespace Dargon.Trinkets {
   public interface ManageableBootstrapConfiguration : ReadableBootstrapConfiguration { 
      void SetFlag(string flag);
      void SetProperty(string key, string value);
   }
}