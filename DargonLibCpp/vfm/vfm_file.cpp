#include <algorithm>

#include "vfm_file.hpp"
#include "vfm_sector.hpp"

using namespace dargon;

void vfm_file::assign_sector(std::shared_ptr<vfm_sector> sector) { 
   sectors.insert(std::make_pair(sector->range().start_inclusive, sector));
}

vfm_file::sector_collection::iterator vfm_file::sectors_begin() { 
   return sectors.begin(); 
}

vfm_file::sector_collection::iterator vfm_file::sectors_end() { 
   return sectors.end(); 
}

void vfm_file::read(int64_t offset, int64_t length, uint8_t * buffer, int64_t buffer_offset) {
   auto sectors_to_read = get_sectors_for_range(vfm_sector_range(offset, offset + length));

   for (auto i = 0; i < sectors_to_read->size(); i++) {
      auto sector = sectors_to_read->at(i);
      auto sector_range = sector->range();

      int64_t sector_read_offset = i == 0 ? std::max(0LL, offset - sector_range.start_inclusive) : 0;
      int64_t buffer_write_offset = sector_range.start_inclusive - offset + sector_read_offset;
      int64_t copy_length = i != sectors_to_read->size() - 1 ? sector_range.size() - sector_read_offset : std::min(sector_range.size() - sector_read_offset, length - buffer_write_offset);

      sector->read(sector_read_offset, copy_length, buffer, buffer_offset + buffer_write_offset);
   }
}

int64_t vfm_file::size() {
   return sectors[sectors.size() - 1]->range().end_exclusive;
}

std::unique_ptr<std::vector<std::shared_ptr<vfm_sector>>> vfm_file::get_sectors_for_range(const vfm_sector_range& sector_range) {
   auto collection = std::make_unique<std::vector<std::shared_ptr<vfm_sector>>>();
   for (auto it = sectors.begin(); it != sectors.end(); it++) {
      auto sector = it->second;
      if (sector_range.intersects(sector->range())) {
         collection->emplace_back(sector);
      }
   }
   return collection;
}
