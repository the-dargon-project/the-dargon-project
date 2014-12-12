#include "stdafx.h"
#include <iostream>
#include "util.hpp"
#include "IO/DSP/DSPExNodeSession.hpp"
#include "Init/bootstrap_context.hpp"
#include "../Subsystem.hpp"
#include "../Subsystem.Detours.hpp"
#include "FileSubsystem.hpp"
#include "FileSubsystemTypedefs.hpp"
#include "FileOperationProxy.hpp"
#include "FileOperationProxyFactory.hpp"
#include "DefaultFileOperationProxy.hpp"

using namespace dargon;
using namespace dargon::Subsystems;

const bool kDebugEnabled = true;

FileSubsystem::FileSubsystem() { }

bool FileSubsystem::Initialize()
{
   std::cout << "At FileSubsystem Init with m_initialized" << m_initialized << std::endl;
   if(m_initialized) return true;
   else {
      Subsystem::Initialize();

      // Ensure we've been told to initialize
      if (!s_configuration->IsFlagSet(Configuration::EnableFileSystemHooksFlag)) {
         std::cout << "At FileSubsystem Init but --enable-filesystem-hooks not set" << std::endl;
         return false;
      }

      HMODULE hModuleKernel32 = WaitForModuleHandle("Kernel32.dll");
      InstallCreateEventADetour(hModuleKernel32);
      InstallCreateEventWDetour(hModuleKernel32);
      InstallCreateFileADetour(hModuleKernel32);
      InstallCreateFileWDetour(hModuleKernel32);
      InstallReadFileDetour(hModuleKernel32);
      InstallWriteFileDetour(hModuleKernel32);
      InstallCloseHandleDetour(hModuleKernel32);
      InstallSetFilePointerDetour(hModuleKernel32);
      s_bootstrap_context->io_proxy->__Override(m_trampCreateEventA, m_trampCreateEventW, m_trampCreateFileA, m_trampCreateFileW, m_trampReadFile, m_trampWriteFile, m_trampCloseHandle, m_trampSetFilePointer);
      return true;
   }
}

bool FileSubsystem::Uninitialize()
{
   if(!m_initialized) return true;
   else
   {
      Subsystem::Uninitialize();

      // Ensure we've been told to initialize
      if (!s_configuration->IsFlagSet(Configuration::EnableFileSystemHooksFlag)) {
         std::cout << "At FileSubsystem Uninitialize but --enable-filesystem-hooks not set" << std::endl;
         return false;
      }

      UninstallCreateEventADetour();
      UninstallCreateEventWDetour();
      UninstallCreateFileADetour();
      UninstallCreateFileWDetour();
      UninstallReadFileDetour();
      UninstallWriteFileDetour();
      UninstallCloseHandleDetour();
      UninstallSetFilePointerDetour();
      return true;
   }
}

void FileSubsystem::AddFileOverride(FileIdentifier fileIdentifier, std::shared_ptr<FileOperationProxyFactory> proxyFactory) {
   proxyFactoriesByFileIdentifier.insert(fileIdentifier, proxyFactory);
}

// - static ---------------------------------------------------------------------------------------
concurrent_dictionary<FileIdentifier, std::shared_ptr<FileOperationProxyFactory>, FileIdentifierHash> FileSubsystem::proxyFactoriesByFileIdentifier;
concurrent_dictionary<HANDLE, std::shared_ptr<FileOperationProxy>> FileSubsystem::fileOperationProxiesByHandle;

DIM_IMPL_STATIC_DETOUR(FileSubsystem, CreateEventA, FunctionCreateEventA, "CreateEventA", MyCreateEventA);
DIM_IMPL_STATIC_DETOUR(FileSubsystem, CreateEventW, FunctionCreateEventW, "CreateEventW", MyCreateEventW);
DIM_IMPL_STATIC_DETOUR(FileSubsystem, CreateFileA, FunctionCreateFileA, "CreateFileA", MyCreateFileA);
DIM_IMPL_STATIC_DETOUR(FileSubsystem, CreateFileW, FunctionCreateFileW, "CreateFileW", MyCreateFileW);
DIM_IMPL_STATIC_DETOUR(FileSubsystem, ReadFile, FunctionReadFile, "ReadFile", MyReadFile);
DIM_IMPL_STATIC_DETOUR(FileSubsystem, WriteFile, FunctionWriteFile, "WriteFile", MyWriteFile);
DIM_IMPL_STATIC_DETOUR(FileSubsystem, CloseHandle, FunctionCloseHandle, "CloseHandle", MyCloseHandle);
DIM_IMPL_STATIC_DETOUR(FileSubsystem, SetFilePointer, FunctionSetFilePointer, "SetFilePointer", MySetFilePointer);

HANDLE WINAPI FileSubsystem::MyCreateEventA(LPSECURITY_ATTRIBUTES lpEventAttributes, BOOL bManualReset, BOOL bInitialState, LPCSTR lpName) {
   return m_trampCreateEventA(lpEventAttributes, bManualReset, bInitialState, lpName);
}
HANDLE WINAPI FileSubsystem::MyCreateEventW(LPSECURITY_ATTRIBUTES lpEventAttributes, BOOL bManualReset, BOOL bInitialState, LPCWSTR lpName) {
   return m_trampCreateEventW(lpEventAttributes, bManualReset, bInitialState, lpName);
}

HANDLE WINAPI FileSubsystem::MyCreateFileA(LPCSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile)
{
   auto path = dargon::wide(lpFilePath);
   return MyCreateFileW(path.c_str(), dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
}

HANDLE WINAPI FileSubsystem::MyCreateFileW(LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile)
{
   if (kDebugEnabled) {
      s_logger->Log(
         LL_VERBOSE,
         [=](std::ostream& os){
         os << "CreateFileW:"
            << " lpFilePath: " << dargon::narrow(lpFilePath)
            << " dwDesiredAccess: " << dwDesiredAccess
            << " dwShareMode: " << dwShareMode
            << " lpSecurityAttributes: " << lpSecurityAttributes
            << " dwCreationDisposition: " << dwCreationDisposition
            << " dwFlagsAndAttributes: " << dwFlagsAndAttributes
            << " hTemplateFile: " << hTemplateFile
            << std::endl;
      });
   }

   FileIdentifier fileIdentifier = GetFileIdentifier(lpFilePath);

   auto proxyFactory = proxyFactoriesByFileIdentifier.get_value_or_default(fileIdentifier);
   std::shared_ptr<FileOperationProxy> fileOperationProxy;
   if (proxyFactory != nullptr) {
      fileOperationProxy = proxyFactory->create();
   }
   if (fileOperationProxy == nullptr) {
      fileOperationProxy = std::shared_ptr<FileOperationProxy>(new DefaultFileOperationProxy(s_bootstrap_context->io_proxy));
   }
   
   auto fileHandle = fileOperationProxy->Create(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
   if (fileHandle != INVALID_HANDLE_VALUE) {
      fileOperationProxiesByHandle.insert(fileHandle, fileOperationProxy);
   }
   return fileHandle;
}

BOOL WINAPI FileSubsystem::MyReadFile(HANDLE hFile, LPVOID lpBuffer, DWORD nNumberOfBytesToRead, LPDWORD lpNumberOfBytesRead, LPOVERLAPPED lpOverlapped)
{
   if (kDebugEnabled) {
      s_logger->Log(
         LL_VERBOSE,
         [=](std::ostream& os) {
         os << "ReadFile: hFile: " << hFile
            << " nNumberOfBytesToRead: " << nNumberOfBytesToRead
            << " lpNumberOfBytesRead: " << lpNumberOfBytesRead
            << " lpOverlapped: " << lpOverlapped << std::endl;
      });
   }

   BOOL result;
   auto proxy = fileOperationProxiesByHandle.get_value_or_default(hFile);
   if (proxy == nullptr) {
      result = m_trampReadFile(hFile, lpBuffer, nNumberOfBytesToRead, lpNumberOfBytesRead, lpOverlapped);
   } else {
      result = proxy->Read(lpBuffer, nNumberOfBytesToRead, (uint32_t*)lpNumberOfBytesRead, lpOverlapped);
   }
   return result;
}

BOOL WINAPI FileSubsystem::MyWriteFile(HANDLE hFile, LPCVOID lpBuffer, DWORD nNumberOfBytesToWrite, LPDWORD lpNumberOfBytesWritten, LPOVERLAPPED lpOverlapped) {
   BOOL result;
   auto proxy = fileOperationProxiesByHandle.get_value_or_default(hFile);
   if (proxy == nullptr) {
      result = m_trampWriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped);
   } else {
      result = proxy->Write(lpBuffer, nNumberOfBytesToWrite, (uint32_t*)lpNumberOfBytesWritten, lpOverlapped);
   }
   return result;
}

BOOL WINAPI FileSubsystem::MyCloseHandle(HANDLE hObject)
{
   auto proxy = fileOperationProxiesByHandle.get_value_or_default(hObject);
   if (proxy) {
      fileOperationProxiesByHandle.remove(hObject);
      return proxy->Close();
   } else {
      return m_trampCloseHandle(hObject);
   }
}
DWORD WINAPI FileSubsystem::MySetFilePointer(HANDLE hFile, LONG lDistanceToMove, PLONG lpDistanceToMoveHigh, DWORD dwMoveMethod)
{
   if (kDebugEnabled) {
      s_logger->Log(
         LL_VERBOSE,
         [=](std::ostream& os) {
         os << "SetFilePointer: hFile: " << hFile
            << " lDistanceToMove: " << lDistanceToMove
            << " lpDistanceToMoveHigh: " << lpDistanceToMoveHigh
            << " dwMoveMethod: " << dwMoveMethod << std::endl;
      });
   }
   auto proxy = fileOperationProxiesByHandle.get_value_or_default(hFile);
   if (proxy) {
      return proxy->Seek(lDistanceToMove, (int32_t*)lpDistanceToMoveHigh, dwMoveMethod);
   } else {
      return m_trampSetFilePointer(hFile, lDistanceToMove, lpDistanceToMoveHigh, dwMoveMethod);
   }
}

FileIdentifier FileSubsystem::GetFileIdentifier(LPCWSTR file_path) {
   auto fileHandle = m_trampCreateFileW(file_path, GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
   FileIdentifier fileIdentifier;
   if (fileHandle == INVALID_HANDLE_VALUE) {
      ZeroMemory(&fileIdentifier, sizeof(fileIdentifier));
   } else {
      BY_HANDLE_FILE_INFORMATION fileInfo;
      GetFileInformationByHandle(fileHandle, &fileInfo);

      fileIdentifier.targetFileIndexHigh = fileInfo.nFileIndexHigh;
      fileIdentifier.targetFileIndexLow = fileInfo.nFileIndexLow;
      fileIdentifier.targetVolumeSerialNumber = fileInfo.dwVolumeSerialNumber;
      m_trampCloseHandle(fileHandle);
   }
   return fileIdentifier;
}