#include "stdafx.h"
#include <iostream>
#include <Psapi.h>
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

// Hook-logging in i/o code can lead to deadlock
const bool kEnableInterceptLogging = false;

FileSubsystem::FileSubsystem() : Subsystem() { }

std::mutex mutex;

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
//      InstallCreateEventADetour(hModuleKernel32);
//      InstallCreateEventWDetour(hModuleKernel32);
//      InstallCreateFileADetour(hModuleKernel32);
      InstallCreateFileWDetour(hModuleKernel32);
      InstallReadFileDetour(hModuleKernel32);
      InstallWriteFileDetour(hModuleKernel32);
      InstallCloseHandleDetour(hModuleKernel32);
      //      InstallSetFilePointerDetour(hModuleKernel32);
      //      InstallSetFilePointerExDetour(hModuleKernel32);
      m_trampCreateEventA = nullptr;
      m_trampCreateEventW = nullptr;
      m_trampCreateFileA = nullptr;
//      m_trampCreateFileW = nullptr;
//      m_trampReadFile = nullptr;
//      m_trampWriteFile = nullptr;
//      m_trampCloseHandle = nullptr;
      m_trampSetFilePointer = nullptr;
      m_trampSetFilePointerEx = nullptr;
//      s_bootstrap_context->io_proxy->__Override(m_trampCreateEventA, m_trampCreateEventW, m_trampCreateFileA, m_trampCreateFileW, m_trampReadFile, m_trampWriteFile, m_trampCloseHandle, m_trampSetFilePointer, m_trampSetFilePointerEx);
      s_bootstrap_context->io_proxy->__Override(nullptr, nullptr, nullptr, m_trampCreateFileW, m_trampReadFile, m_trampWriteFile, m_trampCloseHandle, m_trampSetFilePointer, m_trampSetFilePointerEx);

//      auto h1 = CreateFileW(L"C:\\WINDOWS\\SYSTEM32\\tzres.dll", 2147483648, 5, nullptr, 3, 0, nullptr);
//      auto h2 = CreateFileW(L"C:\\WINDOWS\\SYSTEM32\\tzres.dll", 2147483648, 5, nullptr, 3, 0, nullptr);
//      CloseHandle(h1);
//      CloseHandle(h2);
//      std::cout << h1 << " " << h2 << std::endl;
//
//      h1 = CreateFileW(L"C:\\WINDOWS\\SYSTEM32\\tzres.dll", 2147483648, 5, nullptr, 3, 0, nullptr);
//      CloseHandle(h1);
//      h2 = CreateFileW(L"C:\\WINDOWS\\SYSTEM32\\tzres.dll", 2147483648, 5, nullptr, 3, 0, nullptr);
//      CloseHandle(h2);
//      std::cout << h1 << " " << h2 << std::endl;
//      __debugbreak();

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
//   std::lock_guard<std::mutex> guard(mutex);
//   return m_trampCreateFileW(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
   // MountPointManager
//   std::wstring wideFilePath(lpFilePath);
//   if (wideFilePath.find(L"windows") != -1 || 
//       wideFilePath.find(L"Windows") != -1 ||
//       wideFilePath.find(L"MountPointManager") != -1 ||
//       wideFilePath.find(L"microsoft") != -1 ||
//       wideFilePath.find(L"Microsoft") != -1) {
//      return m_trampCreateFileW(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
//   }
//   if (wideFilePath.find(L"tzres") != -1) {
//      __debugbreak();
//   }
   
   return InternalCreateFileW(false, lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
}

BOOL WINAPI FileSubsystem::MyReadFile(HANDLE hFile, LPVOID lpBuffer, DWORD nNumberOfBytesToRead, LPDWORD lpNumberOfBytesRead, LPOVERLAPPED lpOverlapped)
{
//   if (kDebugEnabled) {
//      s_logger->Log(
//         LL_VERBOSE,
//         [=](std::ostream& os) {
//         os << "ReadFile: hFile: " << hFile
//            << " nNumberOfBytesToRead: " << nNumberOfBytesToRead
//            << " lpNumberOfBytesRead: " << lpNumberOfBytesRead
//            << " lpOverlapped: " << lpOverlapped << std::endl;
//      });
//   }

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
   BOOL result = TRUE;
   bool closeHandle = true;
   std::shared_ptr<FileOperationProxy> proxyToClose;
   fileOperationProxiesByHandle.conditional_remove(
      hObject,
      [&](const HANDLE test, std::shared_ptr<FileOperationProxy> existing) {
         if (existing->__DecrementReferenceCount() == 0) {
            if (kEnableInterceptLogging) {
               std::cout << hObject << " T" << ::GetCurrentThreadId() << " CLOSE HANDLE " << std::endl;
            }
            proxyToClose = existing;
            return true;
         } else {
            closeHandle = false;
            return false;
         }
      });
   if (closeHandle) {
      if (proxyToClose) {
         proxyToClose->Close();
      } else {
         result = m_trampCloseHandle(hObject);
      }
   }
   return result;

//   std::lock_guard<std::mutex> guard(mutex);
//   return m_trampCloseHandle(hObject);

//   BOOL result;
//   std::shared_ptr<FileOperationProxy> proxyToClose;
//   if (fileOperationProxiesByHandle.conditional_remove(
//         hObject, [&proxyToClose, hObject](HANDLE existing, std::shared_ptr<FileOperationProxy> proxy) { 
//               char actualFilePath[1024];
//               ZeroMemory(actualFilePath, 1024);
//               GetFinalPathNameByHandleA(hObject, actualFilePath, 1024, 0);
//               std::cout << "CLOSING " << hObject << " WHICH IS " << actualFilePath << std::endl;

//               auto refcount = proxy->__DecrementReferenceCount();
//               if (refcount == 0) {
//                  proxyToClose = proxy;
//                  return true;
//               }
//               return false; })) {
//      if (proxyToClose) {
//         result = proxyToClose->Close();
//      } else {
//         __debugbreak();
//      }
//   } else {
//      result = m_trampCloseHandle(hObject);
//   }
//   return result;

//   auto proxy_ref_dummy = fileOperationProxiesByHandle.get_value_or_default(hObject);
//   if (proxy_ref_dummy) {
//      std::cout << std::dec << hObject << " CLOSE " << proxy_ref_dummy->ToString() << " " << std::endl;
//      BOOL result = FALSE;
//      fileOperationProxiesByHandle.conditional_remove(
//         hObject,
//         [&result](HANDLE existing, std::shared_ptr<FileOperationProxy> proxy) {
//            int remaining_reference_count = proxy->__DecrementReferenceCount();
//            if (remaining_reference_count == 0) {
//               result = proxy->Close();
//               return true;
//            } else {
//               result = TRUE;
//               return false;
//            }
//         }
//      );
//      return result;
//   } else {
//      return m_trampCloseHandle(hObject);
//   }
}

DWORD WINAPI FileSubsystem::MySetFilePointer(HANDLE hFile, LONG lDistanceToMove, PLONG lpDistanceToMoveHigh, DWORD dwMoveMethod)
{
   return m_trampSetFilePointer(hFile, lDistanceToMove, lpDistanceToMoveHigh, dwMoveMethod);

//   if (kDebugEnabled) {
//      s_logger->Log(
//         LL_VERBOSE,
//         [=](std::ostream& os) {
//         os << "SetFilePointer:"
//            << " hFile: " << hFile
//            << " lDistanceToMove: " << lDistanceToMove
//            << " lpDistanceToMoveHigh: " << lpDistanceToMoveHigh
//            << " dwMoveMethod: " << dwMoveMethod
//            << std::endl;
//      });
//   }

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
   return m_trampSetFilePointerEx(hFile, liDistanceToMove, lpNewFilePointer, dwMoveMethod);
   //   if (kDebugEnabled) {
//      s_logger->Log(
//         LL_VERBOSE,
//         [=](std::ostream& os) {
//         os << "SetFilePointerEx: hFile: " << hFile
//            << " liDistanceToMove: " << liDistanceToMove.QuadPart
//            << " lpDistanceToMoveHigh: " << lpNewFilePointer
//            << " dwMoveMethod: " << dwMoveMethod << std::endl;
//      });
//   }
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
   FileIdentifier fileIdentifier;
   ZeroMemory(&fileIdentifier, sizeof(fileIdentifier));
   
   auto fileHandle = m_trampCreateFileW(file_path, GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
   if (fileHandle == INVALID_HANDLE_VALUE) {
      return fileIdentifier;
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
   HANDLE queryFileHandle = m_trampCreateFileW(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
   if (queryFileHandle == INVALID_HANDLE_VALUE) {
      return queryFileHandle;
   }

   auto threadId = ::GetCurrentThreadId();

   std::wstring filePath = lpFilePath;
   if (filePath.find(L"League of Legends") == -1) {
      return queryFileHandle;
   }

   BY_HANDLE_FILE_INFORMATION fileInfo;
   GetFileInformationByHandle(queryFileHandle, &fileInfo);
   CloseHandle(queryFileHandle);

   FileIdentifier fileIdentifier = GetFileIdentifier(lpFilePath);
   fileIdentifier.targetFileIndexHigh = fileInfo.nFileIndexHigh;
   fileIdentifier.targetFileIndexLow = fileInfo.nFileIndexLow;
   fileIdentifier.targetVolumeSerialNumber = fileInfo.dwVolumeSerialNumber;

   std::shared_ptr<FileOperationProxy> proxy;
   auto proxyFactory = proxyFactoriesByFileIdentifier.get_value_or_default(fileIdentifier);
   if (proxyFactory) {
      proxy = proxyFactory->create();
   } else {
      proxy = std::make_shared<DefaultFileOperationProxy>(s_bootstrap_context->io_proxy);
   }

   HANDLE fileHandle = proxy->Create(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
   proxy->tag.initial_thread = ::GetCurrentThreadId();
   if (kEnableInterceptLogging) {
      std::cout << fileHandle << " T" << ::GetCurrentThreadId() << " CREATE FILE " << dargon::narrow(filePath) << std::endl;
   }

   fileOperationProxiesByHandle.add_or_update(
      fileHandle,
      [proxy](const HANDLE add) { proxy->__IncrementReferenceCount(); return proxy; },
      [proxy, &filePath](const HANDLE update, std::shared_ptr<FileOperationProxy> existing) { std::cout << ":( T_T" << dargon::narrow(filePath) << std::endl; return existing; }
    );

   return fileHandle;
}

//   auto proxyFactory = proxyFactoriesByFileIdentifier.get_value_or_default(fileIdentifier);
//   std::shared_ptr<FileOperationProxy> fileOperationProxy = std::shared_ptr<FileOperationProxy>(new DefaultFileOperationProxy(s_bootstrap_context->io_proxy));
   
//   fileOperationProxy->tag.initial_thread = ::GetCurrentThreadId();
//   return m_trampCreateFileW(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

//   auto fileHandle = fileOperationProxy->Create(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
//   
//   if (fileHandle != INVALID_HANDLE_VALUE) {
//      bool isBorked = false;
//      fileOperationProxiesByHandle.add_or_update(
//         fileHandle,
//         [=](HANDLE add) {
//            auto refs = fileOperationProxy->__IncrementReferenceCount();
//            assert(refs == 1);
//            return fileOperationProxy;
//         },
//         [&](HANDLE update, std::shared_ptr<FileOperationProxy> existing) {
//            char actualFilePath[1024];
//            GetFinalPathNameByHandleA(update, actualFilePath, 1024, 0);
//            if (!dargon::iequals(existing->ToString(), fileOperationProxy->ToString())) {
//               std::cout << "BORKED FILE HANDLE COLLISION!"
//                  << "Exists: " << existing->ToString() << " TID " << existing->tag.initial_thread
//                  << "New: " << fileOperationProxy->ToString() << " TID " << ::GetCurrentThreadId()
//                  << "Actual: " << actualFilePath << std::endl;
//               __debugbreak();
//               isBorked = true;
//               return existing;
//            } else {
//               std::cout << "OKAY FILE HANDLE COLLISION!"
//                  << "Exists: " << existing->ToString() << " TID " << existing->tag.initial_thread
//                  << "New: " << fileOperationProxy->ToString() << " TID " << ::GetCurrentThreadId()
//                  << "Actual: " << actualFilePath << std::endl;
//               existing->__IncrementReferenceCount();
//               return existing;
//            }
//         }
//      );
//      if (isBorked) {
//         fileHandle = InternalCreateFileW(true, lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
//      }
//   }
//   if (kDebugEnabled) {
//      s_logger->Log(
//         LL_VERBOSE,
//         [=](std::ostream& os) {
//         os << " => handle " << fileHandle << std::endl;
//      });
//   }
//   tls_cfw_recursive_count--;
//   return fileHandle;
//}
