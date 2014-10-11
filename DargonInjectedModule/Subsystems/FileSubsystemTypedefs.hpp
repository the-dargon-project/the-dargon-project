#pragma once

#include <cstddef>   // std::size_t
#include <string>    // std::string
#include <utility>   // std::pair
#include <unordered_map>
#include <Windows.h>

#include "FileOverrideTree.hpp"

struct FileOverrideInstanceDescription {
   HANDLE hReplacementFile;
   Dargon::Subsystems::FileOverrideTree* pOverrideTree;

   FileOverrideInstanceDescription(HANDLE handle, Dargon::Subsystems::FileOverrideTree* overrideTree)
      : hReplacementFile(handle), pOverrideTree(overrideTree)
   {
   }
};

typedef std::unordered_map<HANDLE, FileOverrideInstanceDescription> AdvancedOverrideMap;

#if FALSE
struct DIMOverriddenFileDescriptor
{
   DWORD originalVolumeSerialNumber;
   DWORD originalFileIndexHigh;
   DWORD originalFileIndexLow;

   bool operator==(const DIMOverriddenFileDescriptor& rhs) const
   {
      return this->originalVolumeSerialNumber == rhs.originalVolumeSerialNumber &&
             this->originalFileIndexHigh == rhs.originalFileIndexHigh &&
             this->originalFileIndexLow == rhs.originalFileIndexLow;
   }
};

typedef std::pair<DIMOverriddenFileDescriptor, std::string> DIMFileOverride;
typedef std::unordered_map<DIMOverriddenFileDescriptor, std::string, DIMOverriddenFileDescriptorHash> DIMFileOverrideMap;
#endif

#include <IO/IoProxy.hpp>