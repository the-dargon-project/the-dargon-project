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

FileSubsystem::FileSubsystem() : Subsystem() { }

bool FileSubsystem::Initialize()
{
   std::cout << "At FileSubsystem Init with m_initialized=" << m_initialized << std::endl;
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
      InstallSetFilePointerExDetour(hModuleKernel32);
      s_bootstrap_context->io_proxy->__Override(m_trampCreateEventA, m_trampCreateEventW, m_trampCreateFileA, m_trampCreateFileW, m_trampReadFile, m_trampWriteFile, m_trampCloseHandle, m_trampSetFilePointer, m_trampSetFilePointerEx);
      return true;
   }
}

bool FileSubsystem::Uninitialize()
{
   std::cout << "At FileSubsystem Uninit with m_initialized=" << m_initialized << std::endl;
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
      UninstallSetFilePointerExDetour();
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
DIM_IMPL_STATIC_DETOUR(FileSubsystem, SetFilePointerEx, FunctionSetFilePointerEx, "SetFilePointerEx", MySetFilePointerEx);

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
   // MountPointManager
   std::wstring wideFilePath(lpFilePath);
   if (wideFilePath.find(L"windows") != -1 || 
       wideFilePath.find(L"Windows") != -1 ||
       wideFilePath.find(L"MountPointManager") != -1 ||
       wideFilePath.find(L"microsoft") != -1 ||
       wideFilePath.find(L"Microsoft") != -1) {
//      std::cout << std::dec << "SKIP " << dargon::narrow(lpFilePath) << std::endl;
      return m_trampCreateFileW(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
   }

   return InternalCreateFileW(false, lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
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
   auto proxy_ref_dummy = fileOperationProxiesByHandle.get_value_or_default(hObject);
   if (proxy_ref_dummy) {
//      std::cout << std::dec << hObject << " CLOSE " << proxy_ref_dummy->ToString() << " " << std::endl;
      BOOL result = FALSE;
      fileOperationProxiesByHandle.conditional_remove(
         hObject,
         [&result](HANDLE existing, std::shared_ptr<FileOperationProxy> proxy) {
            int remaining_reference_count = proxy->__DecrementReferenceCount();
            if (remaining_reference_count == 0) {
               result = proxy->Close();
               return true;
            } else {
               result = TRUE;
               return false;
            }
         }
      );
      return result;
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
         os << "SetFilePointer:"
            << " hFile: " << hFile
            << " lDistanceToMove: " << lDistanceToMove
            << " lpDistanceToMoveHigh: " << lpDistanceToMoveHigh
            << " dwMoveMethod: " << dwMoveMethod
            << std::endl;
      });
   }

   LARGE_INTEGER distance;
   if (lpDistanceToMoveHigh == nullptr) {
      distance.QuadPart = lDistanceToMove;
   } else {
      distance.LowPart = lDistanceToMove;
      distance.HighPart = *lpDistanceToMoveHigh;
   }

   LARGE_INTEGER final_position;
   auto proxy = fileOperationProxiesByHandle.get_value_or_default(hFile);
   if (proxy) {
      auto result = proxy->Seek(distance.QuadPart, &final_position.QuadPart, dwMoveMethod);

      if (lpDistanceToMoveHigh != nullptr) {
         *lpDistanceToMoveHigh = final_position.HighPart;
      }

      if (result == INVALID_SET_FILE_POINTER) {
         return INVALID_SET_FILE_POINTER;
      } else {
         return final_position.LowPart;
      }
   } else {
      return m_trampSetFilePointer(hFile, lDistanceToMove, lpDistanceToMoveHigh, dwMoveMethod);
   }
}

DWORD WINAPI FileSubsystem::MySetFilePointerEx(HANDLE hFile, LARGE_INTEGER liDistanceToMove, PLARGE_INTEGER lpNewFilePointer, DWORD dwMoveMethod) {
   if (kDebugEnabled) {
      s_logger->Log(
         LL_VERBOSE,
         [=](std::ostream& os) {
         os << "SetFilePointerEx: hFile: " << hFile
            << " liDistanceToMove: " << liDistanceToMove.QuadPart
            << " lpDistanceToMoveHigh: " << lpNewFilePointer
            << " dwMoveMethod: " << dwMoveMethod << std::endl;
      });
   }
   auto proxy = fileOperationProxiesByHandle.get_value_or_default(hFile);
   if (proxy) {
      auto result = proxy->Seek(liDistanceToMove.QuadPart, &lpNewFilePointer->QuadPart, dwMoveMethod);
      if (result == INVALID_SET_FILE_POINTER) {
         return FALSE;
      } else {
         return TRUE;
      }
   } else {
      return m_trampSetFilePointerEx(hFile, liDistanceToMove, lpNewFilePointer, dwMoveMethod);
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

__declspec(thread) int tls_cfw_recursive_count = 0;

HANDLE WINAPI FileSubsystem::InternalCreateFileW(bool isPermittedRecursion, LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {
   tls_cfw_recursive_count++;
   if (tls_cfw_recursive_count != 1 && !isPermittedRecursion) {
      __debugbreak();
   }

   if (kDebugEnabled) {
      s_logger->Log(
         LL_VERBOSE,
         [=](std::ostream& os) {
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
      if (kDebugEnabled) {
         s_logger->Log(
            LL_VERBOSE,
            [=](std::ostream& os) {
            os << " => custom proxy factory" << std::endl;
         });
      }
      fileOperationProxy = proxyFactory->create();
   }
   if (fileOperationProxy == nullptr) {
      if (kDebugEnabled) {
         s_logger->Log(
            LL_VERBOSE,
            [=](std::ostream& os) {
            os << " => default proxy factory" << std::endl;
         });
      }
      fileOperationProxy = std::shared_ptr<FileOperationProxy>(new DefaultFileOperationProxy(s_bootstrap_context->io_proxy));
   }

   auto fileHandle = fileOperationProxy->Create(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
//   std::cout << std::dec << fileHandle << " CREATE " << dargon::narrow(lpFilePath) << " " << std::endl;
   //   auto collision_proxy = fileOperationProxiesByHandle.get_value_or_default(fileHandle);
   //   if (collision_proxy != nullptr) {
   //      std::cout << "FILE HANDLE COLLISION! " 
   //                << "Exists: " << collision_proxy->ToString()
   //                << "New: " << fileOperationProxy->ToString() << std::endl;
   //      __debugbreak();
   //   }
   if (fileHandle != INVALID_HANDLE_VALUE) {
      bool isBorked = false;
      fileOperationProxiesByHandle.add_or_update(
         fileHandle,
         [=](HANDLE add) {
         auto next_count = fileOperationProxy->__IncrementReferenceCount();
         assert(next_count == 1);
         return fileOperationProxy;
      },
         [&](HANDLE update, std::shared_ptr<FileOperationProxy> existing) {
         if (!dargon::iequals(existing->ToString(), fileOperationProxy->ToString())) {
            std::cout << "BORKED FILE HANDLE COLLISION!"
               << "Exists: " << existing->ToString()
               << "New: " << fileOperationProxy->ToString() << std::endl;
            //               __debugbreak();
            isBorked = true;
            return existing;
         } else {
            auto next_count = existing->__IncrementReferenceCount();
            assert(next_count != 1);
            isBorked = false;
            return existing;
         }
      }
      );
      if (isBorked) {
         fileHandle = InternalCreateFileW(true, lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
      }
   }
   if (kDebugEnabled) {
      s_logger->Log(
         LL_VERBOSE,
         [=](std::ostream& os) {
         os << " => handle " << fileHandle << std::endl;
      });
   }
   tls_cfw_recursive_count--;
   return fileHandle;
}
