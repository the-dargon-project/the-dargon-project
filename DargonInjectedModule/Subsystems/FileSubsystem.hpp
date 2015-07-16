#pragma once

#include "stdafx.h"
#include <unordered_map>
#include <concurrent_dictionary.hpp>
#include "../Subsystem.hpp"
#include "../Subsystem.Detours.hpp"
#include "FileSubsystemTypedefs.hpp"
#include "FileOperationProxy.hpp"
#include "FileOperationProxyFactory.hpp"

namespace dargon { namespace Subsystems {
   class FileSubsystem : public dargon::Subsystem
   {
      static dargon::concurrent_dictionary<FileIdentifier, std::shared_ptr<FileOperationProxyFactory>, FileIdentifierHash> proxyFactoriesByFileIdentifier;
      static dargon::concurrent_dictionary<HANDLE, std::shared_ptr<FileOperationProxy>> fileOperationProxiesByHandle;

   public:
      FileSubsystem();
      bool Initialize() override;
      bool Uninitialize() override;

      void AddFileOverride(FileIdentifier fileIdentifier, std::shared_ptr<FileOperationProxyFactory> proxyFactory);
      
      // - static ---------------------------------------------------------------------------------
   private:
      DIM_DECL_STATIC_DETOUR(FileSubsystem, CreateEventA, FunctionCreateEventA, "CreateEventA", MyCreateEventA);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, CreateEventW, FunctionCreateEventW, "CreateEventW", MyCreateEventW);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, CreateFileA, FunctionCreateFileA, "CreateFileA", MyCreateFileA);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, CreateFileW, FunctionCreateFileW, "CreateFileW", MyCreateFileW);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, ReadFile, FunctionReadFile, "ReadFile", MyReadFile);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, WriteFile, FunctionWriteFile, "WriteFile", MyWriteFile);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, CloseHandle, FunctionCloseHandle, "CloseHandle", MyCloseHandle);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, SetFilePointer, FunctionSetFilePointer, "SetFilePointer", MySetFilePointer);
      DIM_DECL_STATIC_DETOUR(FileSubsystem, SetFilePointerEx, FunctionSetFilePointerEx, "SetFilePointerEx", MySetFilePointerEx);
      
      static HANDLE WINAPI MyCreateEventA(LPSECURITY_ATTRIBUTES lpEventAttributes, BOOL bManualReset, BOOL bInitialState, LPCSTR lpName);
      static HANDLE WINAPI MyCreateEventW(LPSECURITY_ATTRIBUTES lpEventAttributes, BOOL bManualReset, BOOL bInitialState, LPCWSTR lpName);
      static HANDLE WINAPI MyCreateFileA(LPCSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
      static HANDLE WINAPI MyCreateFileW(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
      static BOOL WINAPI MyReadFile(HANDLE hFile, LPVOID lpBuffer, DWORD nNumberOfBytesToRead, LPDWORD lpNumberOfBytesRead, LPOVERLAPPED lpOverlapped);
      static BOOL WINAPI MyWriteFile(HANDLE hFile, LPCVOID lpBuffer, DWORD nNumberOfBytesToWrite, LPDWORD lpNumberOfBytesWritten, LPOVERLAPPED lpOverlapped);
      static BOOL WINAPI MyCloseHandle(HANDLE hObject);
      static DWORD WINAPI MySetFilePointer(HANDLE hFile, LONG lDistanceToMove, PLONG lpDistanceToMoveHigh, DWORD dwMoveMethod);
      static DWORD WINAPI MySetFilePointerEx(HANDLE hFile, LARGE_INTEGER liDistanceToMove, PLARGE_INTEGER lpNewFilePointer, DWORD dwMoveMethod);

      static FileIdentifier GetFileIdentifier(LPCWSTR file_path);
      static HANDLE WINAPI InternalCreateFileW(bool isPermittedRecursion, LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
   };
} }