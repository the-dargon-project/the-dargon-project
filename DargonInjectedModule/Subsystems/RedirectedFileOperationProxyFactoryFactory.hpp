#pragma once
#include "stdafx.h"
#include "IO/IoProxy.hpp"
#include "RedirectedFileOperationProxyFactory.hpp"

namespace dargon { namespace Subsystems {
   class RedirectedFileOperationProxyFactoryFactory {
      std::shared_ptr<dargon::IO::IoProxy> io_proxy;

   public:
      RedirectedFileOperationProxyFactoryFactory(std::shared_ptr<dargon::IO::IoProxy> io_proxy);
      std::shared_ptr<RedirectedFileOperationProxyFactory> create(std::string path);
   };
} }