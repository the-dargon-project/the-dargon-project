#pragma once

#include <memory>
#include <map>

#include "base.hpp"
#include "binary_reader.hpp"

#include "vfm_sector.hpp"

namespace dargon {
   class vfm_file : dargon::noncopyable {
      typedef std::map<int64_t, std::shared_ptr<vfm_sector>> sector_collection;

      sector_collection sectors;

   public:
      void assign_sector(std::shared_ptr<vfm_sector> sector);
      sector_collection::iterator sectors_begin();
      sector_collection::iterator sectors_end();

      int64_t size();
      void read(int64_t offset, int64_t length, uint8_t* buffer, int64_t buffer_offset);

   private:
      std::unique_ptr<std::vector<std::shared_ptr<vfm_sector>>> get_sectors_for_range(const vfm_sector_range& range);
   };
}