#pragma once

#include "stdafx.h"
#include <unordered_map>
#include <concurrent_set.hpp>
#include "../Subsystem.hpp"
#include "../Subsystem.Detours.hpp"
#include "FileSubsystemTypedefs.hpp"
#include "FileOverride.hpp"

namespace dargon { namespace IO { namespace DIM {
   class CommandManager;
} } }


namespace dargon {
   namespace Subsystems {
      class FileSwapTaskHandler;
   }
}

namespace dargon { namespace Subsystems {
   class FileSubsystem : public dargon::Subsystem
   {
   private:
      std::shared_ptr<dargon::IO::DIM::CommandManager> command_manager;
      std::shared_ptr<dargon::Subsystems::FileSwapTaskHandler> file_swap_task_handler;

   public:
      FileSubsystem(std::shared_ptr<dargon::IO::DIM::CommandManager> command_manager);
      bool Initialize() override;
      bool Uninitialize() override;

      void AddFileOverride(FileOverrideTargetDescriptor descriptor, FileOverride fileOverride);
      
      // - static ---------------------------------------------------------------------------------
   private:
      static FileOverrideMap s_fileOverridesMap;
      static AdvancedOverrideMap s_advancedOverridesMap;
      static dargon::concurrent_set<HANDLE> mitmHandles;

      DIM_DECL_STATIC_DETOUR(FileSubsystem, CreateEventA, FunctionCreateEventA, "CreateEventA", MyCreateEventA);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, CreateEventW, FunctionCreateEventW, "CreateEventW", MyCreateEventW);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, CreateFileA, FunctionCreateFileA, "CreateFileA", MyCreateFileA);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, CreateFileW, FunctionCreateFileW, "CreateFileW", MyCreateFileW);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, ReadFile, FunctionReadFile, "ReadFile", MyReadFile);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, WriteFile, FunctionWriteFile, "WriteFile", MyWriteFile);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, CloseHandle, FunctionCloseHandle, "CloseHandle", MyCloseHandle);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, SetFilePointer, FunctionSetFilePointer, "SetFilePointer", MySetFilePointer);
      
      static HANDLE WINAPI MyCreateEventA(LPSECURITY_ATTRIBUTES lpEventAttributes, BOOL bManualReset, BOOL bInitialState, LPCSTR lpName);
      static HANDLE WINAPI MyCreateEventW(LPSECURITY_ATTRIBUTES lpEventAttributes, BOOL bManualReset, BOOL bInitialState, LPCWSTR lpName);
      static HANDLE WINAPI MyCreateFileA(LPCSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
      static HANDLE WINAPI MyCreateFileW(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
      static BOOL WINAPI MyReadFile(HANDLE hFile, LPVOID lpBuffer, DWORD nNumberOfBytesToRead, LPDWORD lpNumberOfBytesRead, LPOVERLAPPED lpOverlapped);
      static BOOL WINAPI MyWriteFile(HANDLE hFile, LPCVOID lpBuffer, DWORD nNumberOfBytesToWrite, LPDWORD lpNumberOfBytesWritten, LPOVERLAPPED lpOverlapped);
      static BOOL WINAPI MyCloseHandle(HANDLE hObject);
      static DWORD WINAPI MySetFilePointer(HANDLE hFile, LONG lDistanceToMove, PLONG lpDistanceToMoveHigh, DWORD dwMoveMethod);
   };
} }