#pragma once

#include "stdafx.h"
#include <memory>
#include "IO/IoProxy.hpp"
#include "FileOperationProxy.hpp"
#include "FileOperationProxyFactory.hpp"

namespace dargon { namespace Subsystems {
   class RedirectedFileOperationProxyFactory : public FileOperationProxyFactory, dargon::noncopyable {
      std::shared_ptr<dargon::IO::IoProxy> io_proxy;
      std::string path;

   public:
      RedirectedFileOperationProxyFactory(std::shared_ptr<dargon::IO::IoProxy> io_proxy, std::string path);
      std::shared_ptr<FileOperationProxy> create() override;
   };
} }