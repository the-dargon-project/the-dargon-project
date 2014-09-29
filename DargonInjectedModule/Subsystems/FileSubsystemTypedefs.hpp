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


//-------------------------------------------------------------------------------------------------
// ::CreateFileA
//-------------------------------------------------------------------------------------------------
typedef HANDLE (WINAPI FunctionCreateFileA)(LPCSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
typedef FunctionCreateFileA* PFunctionCreateFileA;
typedef void (FunctionCreateFileANoCC)(LPCSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
typedef FunctionCreateFileANoCC* PFunctionCreateFileANoCC;

//-------------------------------------------------------------------------------------------------
// ::CreateFileW
//-------------------------------------------------------------------------------------------------
typedef HANDLE (WINAPI FunctionCreateFileW)(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
typedef FunctionCreateFileW* PFunctionCreateFileW;
typedef void (FunctionCreateFileWNoCC)(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
typedef FunctionCreateFileWNoCC* PFunctionCreateFileWNoCC;