#pragma once

#include <string>
#include <unordered_map>
#include <boost/noncopyable.hpp>
#include "Base.hpp"
#include "FileSubsystemTypedefs.hpp"
#include "FileOverrideTree.hpp"

namespace Dargon { namespace Subsystems {
   struct FileOverrideTargetDescriptor {
      DWORD targetVolumeSerialNumber;
      DWORD targetFileIndexHigh;
      DWORD targetFileIndexLow;

      bool operator==(const FileOverrideTargetDescriptor& rhs) const
      {
         return this->targetVolumeSerialNumber == rhs.targetVolumeSerialNumber&&
                this->targetFileIndexHigh == rhs.targetFileIndexHigh &&
                this->targetFileIndexLow == rhs.targetFileIndexLow;
      }
   };

   struct FileOverrideTargetDescriptorHash
   {
      std::size_t operator() (const FileOverrideTargetDescriptor& descriptor) const 
      {
         DWORD hash = 17;
         hash = hash * 23 + descriptor.targetVolumeSerialNumber;
         hash = hash * 23 + descriptor.targetFileIndexHigh;
         hash = hash * 23 + descriptor.targetFileIndexLow;
         return hash;
      }
   };

   // base for full swap and swapping chunk w/ chunk.
   struct FileOverride
   {
      // full swap: open x instead of y, let Windows handle everything else. Fast.
      // chunk swap: open x and y. For some chunks in x, read from chunks in y
      //             result "file" might be bigger than x as well.
      //             can do exactly what fullswap does, but is slower
      bool isFullSwap;
      std::string replacementPath;
      FileOverrideTree* pOverrideTree;
   };

   typedef std::unordered_map<FileOverrideTargetDescriptor, FileOverride, FileOverrideTargetDescriptorHash> FileOverrideMap;
} }