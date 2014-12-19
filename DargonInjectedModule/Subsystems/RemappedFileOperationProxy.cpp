#include "stdafx.h"
#include "RemappedFileOperationProxy.hpp"

using namespace dargon::Subsystems;

RemappedFileOperationProxy::RemappedFileOperationProxy(
   std::shared_ptr<dargon::IO::IoProxy> io_proxy,
   std::shared_ptr<dargon::vfm_file> virtual_file_map
) : io_proxy(io_proxy), virtual_file_map(virtual_file_map), position(0LL), name(L"") {
   static bool first = true;
   if (first) {
      __debugbreak();
      first = false;
   }
}

HANDLE RemappedFileOperationProxy::Create(LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {
   // we open an (unused) handle so that we have an identifier + can lock the file.
   handle = io_proxy->CreateFileW(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
   name.assign(lpFilePath);
   return handle;
}

BOOL RemappedFileOperationProxy::Read(void* buffer, uint32_t byte_count, OUT uint32_t* bytes_read, LPOVERLAPPED lpOverlapped) {
   auto isAsyncRead = lpOverlapped != nullptr;
   auto positionToRead = isAsyncRead ? ((lpOverlapped->OffsetHigh << 32) | lpOverlapped->Offset) : position;
   virtual_file_map->read(positionToRead, byte_count, (uint8_t*)buffer, 0);
   *bytes_read = byte_count;

   if (!isAsyncRead) {
      position += byte_count;
      return true;
   } else {
      ResetEvent(lpOverlapped->hEvent);
      lpOverlapped->Internal = STATUS_PENDING;
      SetEvent(lpOverlapped->hEvent);
      SetLastError(ERROR_IO_PENDING);
      return true;
   }
}

BOOL RemappedFileOperationProxy::Write(const void* lpBuffer, uint32_t byte_count, OUT uint32_t* bytes_written, LPOVERLAPPED lpOverlapped) {
   return false;
}

DWORD RemappedFileOperationProxy::Seek(int64_t distance_to_move, int64_t* new_file_pointer, DWORD dwMoveMethod) {
   int64_t next_position;
   if (dwMoveMethod == FILE_BEGIN) {
      next_position = distance_to_move;
   } else if (dwMoveMethod == FILE_CURRENT) {
      next_position = position + distance_to_move;
   } else if (dwMoveMethod == FILE_END) {
      next_position = virtual_file_map->size() - distance_to_move;
   }

   if (next_position < 0LL) {
      SetLastError(ERROR_NEGATIVE_SEEK);
      return INVALID_SET_FILE_POINTER;
   }
   position = next_position;

   if (new_file_pointer != nullptr) {
      *new_file_pointer = position;
   }

   return (int32_t)(next_position & 0xFFFFFFFF);
}

BOOL RemappedFileOperationProxy::Close() {
   CloseHandle(handle);
   handle = NULL;
   return true;
}