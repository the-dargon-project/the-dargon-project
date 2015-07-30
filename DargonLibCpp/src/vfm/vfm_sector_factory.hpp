#pragma once
#include <memory>
#include "guid.hpp"
#include "vfm_sector.hpp"
#include "io/IoProxy.hpp"

namespace dargon {
   class vfm_sector_factory {
      std::shared_ptr<dargon::IO::IoProxy> io_proxy;

   public:
      vfm_sector_factory(std::shared_ptr<dargon::IO::IoProxy> io_proxy);
      std::shared_ptr<vfm_sector> create(dargon::guid type);
   };
}
