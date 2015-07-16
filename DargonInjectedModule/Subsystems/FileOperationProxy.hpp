#pragma once

#include <atomic>
#include <string>
#include <unordered_map>
#include "base.hpp"
#include "FileSubsystemTypedefs.hpp"

namespace dargon { namespace Subsystems {
   class FileOperationProxy {
   private:
      std::atomic_int ref_count;

   public:
      FileOperationProxy() { 
         // std::atomic in vc11 doesn't support direct-initialization
         ref_count = 0;
      }
      virtual ~FileOperationProxy() { }

      int __IncrementReferenceCount() {
         return ++ref_count;
      }
      int __DecrementReferenceCount() { 
         return --ref_count;
      }

      virtual HANDLE Create(LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) = 0;
      virtual BOOL Read(void* buffer, uint32_t byte_count, OUT uint32_t* bytes_read, LPOVERLAPPED lpOverlapped) = 0;
      virtual BOOL Write(const void* lpBuffer, uint32_t byte_count, OUT uint32_t* bytes_written, LPOVERLAPPED lpOverlapped) = 0;
      virtual DWORD Seek(int64_t distance_to_move, int64_t* new_file_pointer, DWORD dwMoveMethod) = 0;
      virtual BOOL Close() = 0;
      virtual std::string ToString() = 0;
   };
} }