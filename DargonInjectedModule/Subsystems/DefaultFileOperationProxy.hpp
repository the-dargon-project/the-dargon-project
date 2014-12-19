#pragma once

#include "stdafx.h"
#include <memory>
#include "noncopyable.hpp"
#include "FileOperationProxy.hpp"
#include "io/IoProxy.hpp"

namespace dargon { namespace Subsystems {
   class DefaultFileOperationProxy : public FileOperationProxy, dargon::noncopyable {
   protected:
      HANDLE handle;
      std::shared_ptr<dargon::IO::IoProxy> io_proxy;

   public:
      DefaultFileOperationProxy(std::shared_ptr<dargon::IO::IoProxy> io_proxy);
      HANDLE Create(LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) override;
      BOOL Read(void* buffer, uint32_t byte_count, OUT uint32_t* bytes_read, LPOVERLAPPED lpOverlapped) override;
      BOOL Write(const void* lpBuffer, uint32_t byte_count, OUT uint32_t* bytes_written, LPOVERLAPPED lpOverlapped) override;
      DWORD Seek(int64_t distance_to_move, int64_t* new_file_pointer, DWORD dwMoveMethod) override;
      BOOL Close() override;
   };
} }