#pragma once
#include <memory>
#include "guid.hpp"
#include "vfm_sector.hpp"

namespace dargon {
   class vfm_sector_factory {
   public:
      std::shared_ptr<vfm_sector> create(dargon::guid type);
   };
}
