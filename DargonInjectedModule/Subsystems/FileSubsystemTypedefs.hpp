#pragma once

#include <cstddef>   // std::size_t
#include <string>    // std::string
#include <utility>   // std::pair
#include <unordered_map>
#include <Windows.h>

namespace dargon { namespace Subsystems {
   struct FileIdentifier {
      DWORD targetVolumeSerialNumber;
      DWORD targetFileIndexHigh;
      DWORD targetFileIndexLow;

      bool operator==(const FileIdentifier& rhs) const {
         return this->targetVolumeSerialNumber == rhs.targetVolumeSerialNumber&&
            this->targetFileIndexHigh == rhs.targetFileIndexHigh &&
            this->targetFileIndexLow == rhs.targetFileIndexLow;
      }
   };

   struct FileIdentifierHash {
      std::size_t operator() (const FileIdentifier& descriptor) const {
         DWORD hash = 17;
         hash = hash * 23 + descriptor.targetVolumeSerialNumber;
         hash = hash * 23 + descriptor.targetFileIndexHigh;
         hash = hash * 23 + descriptor.targetFileIndexLow;
         return hash;
      }
   };
} }