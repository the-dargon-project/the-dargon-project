#pragma once

#include <string>
#include <unordered_map>
#include "base.hpp"
#include "FileSubsystemTypedefs.hpp"

namespace dargon { namespace Subsystems {
   class FileOperationProxy {
   public:
      virtual HANDLE Create(LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) = 0;
      virtual BOOL Read(void* buffer, uint32_t byte_count, OUT uint32_t* bytes_read, LPOVERLAPPED lpOverlapped) = 0;
      virtual BOOL Write(const void* lpBuffer, uint32_t byte_count, OUT uint32_t* bytes_written, LPOVERLAPPED lpOverlapped) = 0;
      virtual DWORD Seek(int32_t distance_to_move, int32_t* distance_to_move_high, DWORD dwMoveMethod) = 0;
      virtual BOOL Close() = 0;
   };
} }