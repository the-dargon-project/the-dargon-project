#pragma once

#include "stdafx.h"
#include <unordered_map>
#include "../Subsystem.hpp"
#include "../Subsystem.Detours.hpp"
#include "FileSubsystemTypedefs.hpp"
#include "FileOverride.hpp"

namespace Dargon { namespace Subsystems {
   class FileSubsystem : public Dargon::Subsystem
   {
      // - singleton ------------------------------------------------------------------------------
   private:
      static FileSubsystem* s_instance;
   public:
      static FileSubsystem* GetInstance();
      
      // - instance -------------------------------------------------------------------------------
   private:
      FileSubsystem();

   public:
      bool Initialize() override;
      bool Uninitialize() override;

      void AddFileOverride(FileOverrideTargetDescriptor descriptor, FileOverride fileOverride);
      
      // - static ---------------------------------------------------------------------------------
   private:
      static FileOverrideMap s_fileOverridesMap;
      static AdvancedOverrideMap s_advancedOverridesMap;
      
      DIM_DECL_STATIC_DETOUR(FileSubsystem, CreateFileA, FunctionCreateFileA, "CreateFileA", MyCreateFileA);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, CreateFileW, FunctionCreateFileW, "CreateFileW", MyCreateFileW);

      static HANDLE WINAPI MyCreateFileA(LPCSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
      static HANDLE WINAPI MyCreateFileW(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
   };
} }