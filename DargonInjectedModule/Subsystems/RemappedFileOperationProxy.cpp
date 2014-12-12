#include "stdafx.h"
#include "RemappedFileOperationProxy.hpp"

using namespace dargon::Subsystems;

RemappedFileOperationProxy::RemappedFileOperationProxy(
   std::shared_ptr<dargon::IO::IoProxy> io_proxy,
   std::shared_ptr<dargon::vfm_file> virtual_file_map
) : io_proxy(io_proxy), virtual_file_map(virtual_file_map), position(0LL) {
}

HANDLE RemappedFileOperationProxy::Create(LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {
   // we open an (unused) handle so that we have an identifier + can lock the file.
   handle = io_proxy->CreateFileW(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
   return handle;
}

BOOL RemappedFileOperationProxy::Read(void* buffer, uint32_t byte_count, OUT uint32_t* bytes_read, LPOVERLAPPED lpOverlapped) {
   virtual_file_map->read(position, byte_count, (uint8_t*)buffer, 0);
   position += byte_count;
   *bytes_read = byte_count;
   return true;
}

BOOL RemappedFileOperationProxy::Write(const void* lpBuffer, uint32_t byte_count, OUT uint32_t* bytes_written, LPOVERLAPPED lpOverlapped) {
   return false;
}

DWORD RemappedFileOperationProxy::Seek(int32_t distance_to_move, int32_t* distance_to_move_high, DWORD dwMoveMethod) {
   int64_t value = distance_to_move;
   if (distance_to_move_high) {
      value |= (*distance_to_move_high) << 32;
   }

   int64_t next_position;
   if (dwMoveMethod == FILE_BEGIN) {
      next_position = value;
   } else if (dwMoveMethod == FILE_CURRENT) {
      next_position = position + value;
   } else if (dwMoveMethod == FILE_END) {
      next_position = virtual_file_map->size() - value;
   }

   if (next_position < 0) {
      return ERROR_NEGATIVE_SEEK;
   } 
   if (distance_to_move_high == nullptr && next_position > UINT32_MAX) {
      return INVALID_SET_FILE_POINTER;
   }
   position = next_position;
   return next_position & 0xFFFFFFFFUL;
}

BOOL RemappedFileOperationProxy::Close() {
   CloseHandle(handle);
   handle = NULL;
   return true;
}