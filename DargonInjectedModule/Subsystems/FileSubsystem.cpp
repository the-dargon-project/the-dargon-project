#include "stdafx.h"
#include <boost/nowide/convert.hpp>
#include "IO/DSP/DSPExNodeSession.hpp"
#include "Init/BootstrapContext.hpp"
#include "../Subsystem.hpp"
#include "../Subsystem.Detours.hpp"
#include "FileSubsystem.hpp"
#include "FileSubsystemTypedefs.hpp"
#include "FileOverrideTaskHandler.hpp"
#include "FileSwapTaskHandler.hpp"

using namespace Dargon::Subsystems;

const bool kDebugEnabled = false;

// - singleton ------------------------------------------------------------------------------------
FileSubsystem* FileSubsystem::s_instance = nullptr;
FileSubsystem* FileSubsystem::GetInstance()
{
   if(s_instance == nullptr)
      s_instance = new FileSubsystem();
   return s_instance;
}

// - instance -------------------------------------------------------------------------------------
FileSubsystem::FileSubsystem() 
{
   // Register DIM Task Handlers
   auto dimTaskManager = s_core->GetDIMTaskManager();
   if (dimTaskManager)
   {
      dimTaskManager->RegisterTaskHandler(new FileSwapTaskHandler(this));
   }
}

bool FileSubsystem::Initialize()
{
   std::cout << "At FileSubsystem Init with m_initialized" << m_initialized << std::endl;
   if(m_initialized) return true;
   else
   {
      Subsystem::Initialize();

      // Ensure we've been told to initialize
      if(std::find(s_bootstrapContext->ArgumentFlags.begin(),
                   s_bootstrapContext->ArgumentFlags.end(),
                   "--enable-filesystem-hooks") == s_bootstrapContext->ArgumentFlags.end())
      {
         std::cout << "At FileSubsystem Init but --enable-filesystem-hooks not set" << std::endl;
         return false;
      }

      HMODULE hModuleKernel32 = WaitForModuleHandle("Kernel32.dll");
      InstallCreateFileADetour(hModuleKernel32);
      InstallCreateFileWDetour(hModuleKernel32);
      return true;
   }
}

bool FileSubsystem::Uninitialize()
{
   if(!m_initialized) return true;
   else
   {
      Subsystem::Uninitialize();
      return true;
   }
}

void FileSubsystem::AddFileOverride(FileOverrideTargetDescriptor descriptor, FileOverride fileOverride)
{
   s_fileOverridesMap.insert(
      FileOverrideMap::value_type(
         descriptor,
         fileOverride
      )
   );
}

// - static ---------------------------------------------------------------------------------------
FileOverrideMap FileSubsystem::s_fileOverridesMap;
AdvancedOverrideMap FileSubsystem::s_advancedOverridesMap;

DIM_IMPL_STATIC_DETOUR(FileSubsystem, CreateFileA, FunctionCreateFileA, "CreateFileA", MyCreateFileA);
DIM_IMPL_STATIC_DETOUR(FileSubsystem, CreateFileW, FunctionCreateFileW, "CreateFileW", MyCreateFileW);

HANDLE WINAPI FileSubsystem::MyCreateFileA(LPCSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile)
{
   if (kDebugEnabled)
      std::cout << "Detour MyCreateFileA:" 
                << " lpFilePath: " << lpFilePath
                << " dwDesiredAccess: " << dwDesiredAccess
                << " dwShareMode: " << dwShareMode
                << " lpSecurityAttributes: " << lpSecurityAttributes
                << " dwCreationDisposition: " << dwCreationDisposition
                << " dwFlagsAndAttributes: " << dwFlagsAndAttributes
                << " hTemplateFile: " << hTemplateFile
                << std::endl;
   auto fileHandle = m_trampCreateFileA(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

   BY_HANDLE_FILE_INFORMATION fileInfo;
   GetFileInformationByHandle(fileHandle, &fileInfo);
   
   FileOverrideTargetDescriptor descriptor;
   descriptor.targetFileIndexHigh = fileInfo.nFileIndexHigh;
   descriptor.targetFileIndexLow = fileInfo.nFileIndexLow;
   descriptor.targetVolumeSerialNumber = fileInfo.dwVolumeSerialNumber;

   auto override = s_fileOverridesMap.find(descriptor);
   if(override != s_fileOverridesMap.end())
   {
      if (override->second.isFullSwap)
      {
         CloseHandle(fileHandle);
         fileHandle = m_trampCreateFileA(override->second.replacementPath.c_str(), dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
      }
      else // advanced override
      {
         auto tree = override->second.pOverrideTree;
         HANDLE fileHandleReplacement = m_trampCreateFileA(override->second.replacementPath.c_str(), dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

         // If the replacement doesn't exist, nothing special happens. Otherwise, 
         // register advanced override.
         if (fileHandleReplacement != INVALID_HANDLE_VALUE)
         {
            s_advancedOverridesMap.insert(
               AdvancedOverrideMap::value_type(
                  fileHandle,
                  FileOverrideInstanceDescription(fileHandleReplacement, tree)
               )
            );
         }
      }
   }
   return fileHandle;
}

HANDLE WINAPI FileSubsystem::MyCreateFileW(LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile)
{
   if (kDebugEnabled)
      std::cout << "Detour CreateFileW:" 
                << " lpFilePath: " << boost::nowide::narrow(lpFilePath)
                << " dwDesiredAccess: " << dwDesiredAccess
                << " dwShareMode: " << dwShareMode
                << " lpSecurityAttributes: " << lpSecurityAttributes
                << " dwCreationDisposition: " << dwCreationDisposition
                << " dwFlagsAndAttributes: " << dwFlagsAndAttributes
                << " hTemplateFile: " << hTemplateFile
                << std::endl;
   auto fileHandle = m_trampCreateFileW(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

   BY_HANDLE_FILE_INFORMATION fileInfo;
   GetFileInformationByHandle(fileHandle, &fileInfo);
   
   FileOverrideTargetDescriptor descriptor;
   descriptor.targetFileIndexHigh = fileInfo.nFileIndexHigh;
   descriptor.targetFileIndexLow = fileInfo.nFileIndexLow;
   descriptor.targetVolumeSerialNumber = fileInfo.dwVolumeSerialNumber;

   auto override = s_fileOverridesMap.find(descriptor);
   if(override != s_fileOverridesMap.end())
   {
      if (override->second.isFullSwap)
      {
         CloseHandle(fileHandle);
         fileHandle = m_trampCreateFileA(override->second.replacementPath.c_str(), dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
      }
      else // advanced override
      {
         auto tree = override->second.pOverrideTree;
         HANDLE fileHandleReplacement = m_trampCreateFileA(override->second.replacementPath.c_str(), dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

         // If the replacement doesn't exist, nothing special happens. Otherwise, 
         // register advanced override.
         if (fileHandleReplacement != INVALID_HANDLE_VALUE)
         {
            s_advancedOverridesMap.insert(
               AdvancedOverrideMap::value_type(
                  fileHandle,
                  FileOverrideInstanceDescription(fileHandleReplacement, tree)
               )
            );
         }
      }
   }
   return fileHandle;
}
