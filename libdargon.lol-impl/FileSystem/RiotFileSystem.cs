using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.IO.RADS;

namespace Dargon.FileSystem
{
   public class RiotFileSystem : IFileSystem
   {
      private readonly string solutionPath;
      private readonly RiotProjectType projectType;
      private RiotProject project;

      public RiotFileSystem(string solutionPath, RiotProjectType projectType)
      {
         this.solutionPath = solutionPath;
         this.projectType = projectType;

         this.Initialize();
      }

      private void Initialize() {
         project = new RiotSolutionLoader().Load(solutionPath, projectType).ProjectsByType[projectType]; 
      }

      public IFileSystemHandle AllocateRootHandle() { throw new NotImplementedException(); }
      public IoResult AllocateChildrenHandles(IFileSystemHandle handles, out IFileSystemHandle[] childHandles) { throw new NotImplementedException(); }
      public IoResult ReadAllBytes(IFileSystemHandle handle, out byte[] bytes) { throw new NotImplementedException(); }
      public void ReturnHandle(IFileSystemHandle handle) { throw new NotImplementedException(); }
      public void Suspend() { throw new NotImplementedException(); }
      public void Resume() { throw new NotImplementedException(); }
   }
}
