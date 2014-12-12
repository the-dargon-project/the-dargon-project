#include "stdafx.h"
#include "RedirectedFileOperationProxyFactory.hpp"
#include "RedirectedFileOperationProxy.hpp"

using namespace dargon::IO;
using namespace dargon::Subsystems;

RedirectedFileOperationProxyFactory::RedirectedFileOperationProxyFactory(std::shared_ptr<IoProxy> io_proxy, std::string path)
   : io_proxy(io_proxy), path(path) { }

std::shared_ptr<FileOperationProxy> RedirectedFileOperationProxyFactory::create() {
   return std::shared_ptr<FileOperationProxy>(new RedirectedFileOperationProxy(io_proxy, path));
}
