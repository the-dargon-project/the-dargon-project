#include "stdafx.h"
#include "RemappedFileOperationProxy.hpp"

using namespace dargon::Subsystems;

RemappedFileOperationProxy::RemappedFileOperationProxy(
   std::shared_ptr<dargon::IO::IoProxy> io_proxy,
   std::shared_ptr<dargon::vfm_file> virtual_file_map
) : io_proxy(io_proxy), virtual_file_map(virtual_file_map), name(""), handle(INVALID_HANDLE_VALUE) {
   static bool first = true;
   if (first) {
//      __debugbreak();
      first = false;
   }
}
HANDLE RemappedFileOperationProxy::Create(LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {
   if (handle != INVALID_HANDLE_VALUE) {
      std::cout << "Create invoked on Remapped File Proxy more than once!" << std::endl;
      __debugbreak();
      return INVALID_HANDLE_VALUE;
   } else {
      // we open an (unused) handle so that we have an identifier + can lock the file.
      DWORD additional_share_flags = 0;
      if (((dwDesiredAccess & GENERIC_READ) != 0) && ((dwDesiredAccess & GENERIC_WRITE) == 0)) {
         additional_share_flags |= FILE_SHARE_READ;
      }
      handle = io_proxy->CreateFileW(lpFilePath, dwDesiredAccess, dwShareMode | additional_share_flags, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
      name.assign(dargon::narrow(lpFilePath));
      return handle;
   }
}

//std::mutex g_readLock;
BOOL RemappedFileOperationProxy::Read(void* buffer, uint32_t byte_count, OUT uint32_t* bytes_read, LPOVERLAPPED lpOverlapped) {
   if (lpOverlapped != nullptr) {
      // Synchronous method: This seems to not be supported by League of Legends?
//      LARGE_INTEGER li;
//      li.LowPart = lpOverlapped->Offset;
//      li.HighPart = lpOverlapped->OffsetHigh;
//      std::cout << "Overlapped read at " << li.QuadPart << " with length " << byte_count << std::endl;
//      auto actual_bytes_read = virtual_file_map->read(li.QuadPart, byte_count, (uint8_t*)buffer, 0);
//      if (bytes_read != nullptr) {
//         *bytes_read = actual_bytes_read;
//      }
//      return TRUE;

      // Asynchronous method: Leaks a bit of memory.
      LARGE_INTEGER li;
      li.LowPart = lpOverlapped->Offset;
      li.HighPart = lpOverlapped->OffsetHigh;
      auto actual_bytes_read = virtual_file_map->read(li.QuadPart, byte_count, (uint8_t*)buffer, 0);
      if (bytes_read != nullptr) {
         *bytes_read = actual_bytes_read;
      }
      
      auto hDummy = io_proxy->CreateFileW(dargon::wide(name).c_str(), GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
      auto file_length = SetFilePointer(hDummy, 0, nullptr, FILE_END);
      io_proxy->CloseHandle(hDummy);

      if (file_length < actual_bytes_read) {
         __debugbreak();
      }

      LARGE_INTEGER offset_to_start_from;
      offset_to_start_from.QuadPart = 0; 

      uint8_t* memory_leak = new uint8_t[actual_bytes_read];
      BOOL isDummyOperationSynchronous = TRUE;
      while (isDummyOperationSynchronous) {
         lpOverlapped->Offset = offset_to_start_from.LowPart;
         lpOverlapped->OffsetHigh = offset_to_start_from.HighPart;
         DWORD dummy_bytes_read = 0;
         isDummyOperationSynchronous = io_proxy->ReadFile(handle, memory_leak, actual_bytes_read, &dummy_bytes_read, lpOverlapped);
      }
      SetLastError(ERROR_IO_PENDING);
      return FALSE;
   } else {
      LARGE_INTEGER zero;
      zero.QuadPart = 0;

      LARGE_INTEGER initial_file_pointer;
      initial_file_pointer.QuadPart = 0;

      auto ifpSetFileResult = io_proxy->SetFilePointerEx(handle, zero, &initial_file_pointer, FILE_CURRENT);
      if (ifpSetFileResult == 0) {
         auto err = GetLastError();
         std::cout << "SetFilePoinerEx failed with err " << std::dec << err << std::endl;
         __debugbreak();
      }
      
      auto actual_bytes_read = virtual_file_map->read(initial_file_pointer.QuadPart, byte_count, (uint8_t*)buffer, 0);

      LARGE_INTEGER move;
      move.QuadPart = actual_bytes_read;

      LARGE_INTEGER final_location;
      final_location.QuadPart = 0;
      
      auto setFileResult = io_proxy->SetFilePointerEx(handle, move, &final_location, FILE_CURRENT);
      if (setFileResult == 0) {
         auto err = GetLastError();
         std::cout << "SetFilePoinerEx failed with err " << std::dec << err << std::endl;
         __debugbreak();
      }

      if (bytes_read != nullptr) {
         *bytes_read = actual_bytes_read;
      }
      return TRUE;
   }
}

BOOL RemappedFileOperationProxy::Write(const void* lpBuffer, uint32_t byte_count, OUT uint32_t* bytes_written, LPOVERLAPPED lpOverlapped) {
   __debugbreak();
   return false;
}

DWORD RemappedFileOperationProxy::Seek(int64_t distance_to_move, int64_t* new_file_pointer, DWORD dwMoveMethod) {
   if (dwMoveMethod == FILE_END) {
      if (distance_to_move != 0) {
         __debugbreak();
         return 0;
      } else {
         auto vfm_size = virtual_file_map->size();

         LARGE_INTEGER li_distance_to_move;
         li_distance_to_move.QuadPart = vfm_size;

         LARGE_INTEGER li_new_position;
         li_new_position.QuadPart = 0;
         auto result = io_proxy->SetFilePointerEx(handle, li_distance_to_move, &li_new_position, FILE_BEGIN);

         if (li_new_position.QuadPart != vfm_size || result == 0) {
            __debugbreak();
         }

         if (new_file_pointer != nullptr) {
            *new_file_pointer = li_new_position.QuadPart;
         }

         return result;
      }
   } else {
      LARGE_INTEGER d2m;
      d2m.QuadPart = distance_to_move;
      LARGE_INTEGER newOffset;
      newOffset.QuadPart = 0;
      auto result = io_proxy->SetFilePointerEx(handle, d2m, &newOffset, dwMoveMethod);
      if (new_file_pointer != nullptr) {
         *new_file_pointer = newOffset.QuadPart;
      }
      return result;
   }
}

BOOL RemappedFileOperationProxy::Close() {
   io_proxy->CloseHandle(handle);
   handle = INVALID_HANDLE_VALUE;
   return true;
}

std::string RemappedFileOperationProxy::ToString() { return name; }