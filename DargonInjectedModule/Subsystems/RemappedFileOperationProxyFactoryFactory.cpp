#include "stdafx.h"
#include <fstream>
#include "RemappedFileOperationProxyFactoryFactory.hpp"

using namespace dargon::Subsystems;

RemappedFileOperationProxyFactoryFactory::RemappedFileOperationProxyFactoryFactory(
   std::shared_ptr<dargon::IO::IoProxy> io_proxy,
   std::shared_ptr<dargon::vfm_reader> virtual_file_map_reader
) : io_proxy(io_proxy), virtual_file_map_reader(virtual_file_map_reader) { 
}

std::shared_ptr<RemappedFileOperationProxyFactory> RemappedFileOperationProxyFactoryFactory::create(std::string vfm_path) {
   auto fs = std::make_shared<std::fstream>();
   fs->open(vfm_path.c_str(), std::fstream::in | std::fstream::binary);
   binary_reader reader(fs);
   auto vfm = virtual_file_map_reader->load(reader);
   fs->close();
   return std::make_shared<RemappedFileOperationProxyFactory>(io_proxy, vfm);
}
