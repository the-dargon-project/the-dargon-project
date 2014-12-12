#pragma once

#include "stdafx.h"
#include <memory>
#include "noncopyable.hpp"
#include "FileOperationProxy.hpp"
#include "DefaultFileOperationProxy.hpp"

namespace dargon { namespace Subsystems {
   class RedirectedFileOperationProxy : public DefaultFileOperationProxy {
   private:
      std::string path;

   public:
      RedirectedFileOperationProxy(std::shared_ptr<dargon::IO::IoProxy> io_proxy, std::string path);

      HANDLE Create(LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) override;
   };
} }