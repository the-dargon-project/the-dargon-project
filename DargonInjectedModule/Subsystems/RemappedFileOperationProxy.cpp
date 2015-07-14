#include "stdafx.h"
#include "RemappedFileOperationProxy.hpp"

using namespace dargon::Subsystems;

RemappedFileOperationProxy::RemappedFileOperationProxy(
   std::shared_ptr<dargon::IO::IoProxy> io_proxy,
   std::shared_ptr<dargon::vfm_file> virtual_file_map
) : io_proxy(io_proxy), virtual_file_map(virtual_file_map), position(0LL), name(L"") {
   static bool first = true;
   if (first) {
//      __debugbreak();
      first = false;
   }
}
HANDLE RemappedFileOperationProxy::Create(LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {
   // we open an (unused) handle so that we have an identifier + can lock the file.
   DWORD additional_share_flags = 0;
   if (((dwDesiredAccess & GENERIC_READ) != 0) && ((dwDesiredAccess & GENERIC_WRITE) == 0)) {
      additional_share_flags |= FILE_SHARE_READ;
   }
   handle = io_proxy->CreateFileW(lpFilePath, dwDesiredAccess, dwShareMode | additional_share_flags, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
   name.assign(lpFilePath);
//   std::cout << handle << " CREATE " << lpFilePath << std::endl;
   return handle;
}

//std::mutex g_readLock;
BOOL RemappedFileOperationProxy::Read(void* buffer, uint32_t byte_count, OUT uint32_t* bytes_read, LPOVERLAPPED lpOverlapped) {
//   std::lock_guard<std::mutex> synchronization(g_readLock);
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
      virtual_file_map->read(li.QuadPart, byte_count, (uint8_t*)buffer, 0);
      uint8_t* memory_leak = new uint8_t[byte_count];
      BOOL isDummyOperationSynchronous = TRUE;
      while (isDummyOperationSynchronous) {
         lpOverlapped->Offset = 0;
         lpOverlapped->OffsetHigh = 0;
         DWORD dummy_bytes_read = 0;
         isDummyOperationSynchronous = io_proxy->ReadFile(handle, memory_leak, byte_count, &dummy_bytes_read, lpOverlapped);
      }
      SetLastError(ERROR_IO_PENDING);
      return FALSE;
   } else {
      LARGE_INTEGER move;
      move.QuadPart = 0;

      LARGE_INTEGER li;
      io_proxy->SetFilePointerEx(handle, move, &li, FILE_CURRENT);

      auto actual_bytes_read = virtual_file_map->read(li.QuadPart, byte_count, (uint8_t*)buffer, 0);
      if (bytes_read != nullptr) {
         *bytes_read = actual_bytes_read;
      }
      return TRUE;
   }
}

BOOL RemappedFileOperationProxy::Write(const void* lpBuffer, uint32_t byte_count, OUT uint32_t* bytes_written, LPOVERLAPPED lpOverlapped) {
   return false;
}

DWORD RemappedFileOperationProxy::Seek(int64_t distance_to_move, int64_t* new_file_pointer, DWORD dwMoveMethod) {
   LARGE_INTEGER d2m;
   d2m.QuadPart = distance_to_move;
   LARGE_INTEGER newOffset;
   newOffset.QuadPart = 0;
   auto result = io_proxy->SetFilePointerEx(handle, d2m, &newOffset, dwMoveMethod);
   if (new_file_pointer != nullptr) {
      *new_file_pointer = newOffset.QuadPart;
   }
   position = newOffset.QuadPart;
   return result;
   int64_t next_position;
   if (dwMoveMethod == FILE_BEGIN) {
      next_position = distance_to_move;
   } else if (dwMoveMethod == FILE_CURRENT) {
      next_position = position + distance_to_move;
   } else if (dwMoveMethod == FILE_END) {
      next_position = virtual_file_map->size() + distance_to_move;
   }

//   std::cout << handle << " SEEK " << distance_to_move << " mode: " << dwMoveMethod  << " yields " << position << " => " << next_position << std::endl;

   if (next_position < 0LL) {
      SetLastError(ERROR_NEGATIVE_SEEK);
      return INVALID_SET_FILE_POINTER;
   }
   position = next_position;

   if (new_file_pointer != nullptr) {
      *new_file_pointer = position;
   }

   return (int32_t)(next_position & 0xFFFFFFFFU);
}

BOOL RemappedFileOperationProxy::Close() {
//   std::cout << handle << " CLOSE" << std::endl;
   CloseHandle(handle);
   handle = NULL;
   return true;
}