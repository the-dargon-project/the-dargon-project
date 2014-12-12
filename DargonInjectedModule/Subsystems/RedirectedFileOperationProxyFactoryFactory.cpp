#include "stdafx.h"
#include "RedirectedFileOperationProxyFactoryFactory.hpp"

using namespace dargon::Subsystems;

RedirectedFileOperationProxyFactoryFactory::RedirectedFileOperationProxyFactoryFactory(
   std::shared_ptr<dargon::IO::IoProxy> io_proxy
) : io_proxy(io_proxy) {
}

std::shared_ptr<RedirectedFileOperationProxyFactory> RedirectedFileOperationProxyFactoryFactory::create(std::string path) {
   return std::make_shared<RedirectedFileOperationProxyFactory>(io_proxy, path);
}
