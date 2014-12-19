#include "dlc_pch.hpp"
#include "vfm_sector_factory.hpp"
#include "vfm_file_sector.hpp"

using namespace dargon;

vfm_sector_factory::vfm_sector_factory(std::shared_ptr<dargon::IO::IoProxy> io_proxy) : io_proxy(io_proxy) {}

std::shared_ptr<vfm_sector> vfm_sector_factory::create(dargon::guid type) {
   if (type == vfm_file_sector::kGuid) {
      return std::shared_ptr<vfm_sector>(new vfm_file_sector(io_proxy));
   } else {
      std::cout << "Did not have for guid " << type.to_string() << " didn't match " << vfm_file_sector::kGuid.to_string() << std::endl;
      throw std::exception("vfm sector type not supported");
   }
}
