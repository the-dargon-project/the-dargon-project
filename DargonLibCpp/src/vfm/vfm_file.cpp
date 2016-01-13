#include <algorithm>
#include <cassert>

#include "vfm_file.hpp"
#include "vfm_sector.hpp"

using namespace dargon;

void vfm_file::assign_sector(vfm_sector_range sector_range, std::shared_ptr<vfm_sector> sector) { 
   sectors.push_back(std::make_pair(sector_range, sector));
}

vfm_file::sector_collection::iterator vfm_file::sectors_begin() { 
   return sectors.begin(); 
}

vfm_file::sector_collection::iterator vfm_file::sectors_end() { 
   return sectors.end(); 
}

int64_t vfm_file::read(int64_t offset, int64_t length, uint8_t * buffer, int64_t buffer_offset) {
   if (buffer_offset != 0) {
      return this->read(offset, length, buffer + buffer_offset, 0);
   }
   auto bytesRead = std::min(size() - offset, length);
//   std::cout << "I am size " << std::dec << size() << " and we are reading offset " << offset << " length " << length << " yielding br " << bytesRead << std::endl;
   ZeroMemory(buffer, bytesRead);

   auto sectors_to_read = get_sectors_for_range(vfm_sector_range(offset, offset + bytesRead));
   if (sectors_to_read->size() >= 2) {
//      __debugbreak();
   }

   for (auto i = 0; i < sectors_to_read->size(); i++) {
      auto sector_range = sectors_to_read->at(i).first;
      auto sector = sectors_to_read->at(i).second;

      int64_t sector_read_offset = std::max(0LL, offset - sector_range.start_inclusive);
      int64_t buffer_write_offset = (sector_range.start_inclusive + sector_read_offset) - offset;
      int64_t copy_length = std::min(sector_range.size() - sector_read_offset, bytesRead - buffer_write_offset);

      assert(buffer_write_offset >= 0);

//      std::cout << "   sector " << i << " has range [" << sector_range.start_inclusive << ", " << sector_range.end_exclusive << ") " << std::endl;
//      std::cout << "      read offset: " << sector_read_offset << " length " << copy_length << " buffer_offset " << buffer_write_offset << std::endl;

      if (copy_length + buffer_write_offset > bytesRead) {
         std::cout << std::dec << copy_length << " + " << buffer_write_offset << " > " << bytesRead << " =(" << std::endl;
         __debugbreak();
      }

      sector->read(sector_read_offset, copy_length, buffer, buffer_write_offset);
   }
   return bytesRead;
}

int64_t vfm_file::size() {
   return sectors[sectors.size() - 1].first.end_exclusive;
}

std::unique_ptr<std::vector<std::pair<vfm_sector_range, std::shared_ptr<vfm_sector>>>> vfm_file::get_sectors_for_range(const vfm_sector_range& test_range) {
   auto collection = std::make_unique<std::vector<std::pair<vfm_sector_range, std::shared_ptr<vfm_sector>>>>();
   for (auto it = sectors.begin(); it != sectors.end(); it++) {
      auto sector_range = it->first;
      auto sector = it->second;
      if (test_range.intersects(sector_range)) {
         collection->emplace_back(*it);
      }
   }
   return collection;
}
