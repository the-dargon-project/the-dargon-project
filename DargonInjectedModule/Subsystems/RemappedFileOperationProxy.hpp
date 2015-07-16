#pragma once

#include "stdafx.h"
#include <memory>
#include "noncopyable.hpp"
#include "vfm/vfm_file.hpp"
#include "IO/IoProxy.hpp"
#include "FileOperationProxy.hpp"

namespace dargon { namespace Subsystems {
   class RemappedFileOperationProxy : public FileOperationProxy, public noncopyable {
      std::string name;
      std::shared_ptr<dargon::IO::IoProxy> io_proxy;
      std::shared_ptr<dargon::vfm_file> virtual_file_map;
      HANDLE handle;

   public:
      RemappedFileOperationProxy(std::shared_ptr<dargon::IO::IoProxy> io_proxy, std::shared_ptr<dargon::vfm_file> virtual_file_map);
      HANDLE Create(LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) override;
      BOOL Read(void* buffer, uint32_t byte_count, OUT uint32_t* bytes_read, LPOVERLAPPED lpOverlapped) override;
      BOOL Write(const void* lpBuffer, uint32_t byte_count, OUT uint32_t* bytes_written, LPOVERLAPPED lpOverlapped) override;
      DWORD Seek(int64_t distance_to_move, int64_t* new_file_pointer, DWORD dwMoveMethod) override;
      BOOL Close() override;
      std::string ToString() override;
   };
} }