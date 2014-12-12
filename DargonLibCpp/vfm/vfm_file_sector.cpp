#include "dlc_pch.hpp"
#include <fstream>
#include <sstream>
#include "binary_reader.hpp"
#include "vfm_file_sector.hpp"

using namespace dargon;

const dargon::guid vfm_file_sector::kGuid(guid::parse("5DB2B4C239AE4629988ACFFFCE89F230"));

vfm_file_sector::vfm_file_sector() {

}

vfm_sector_range vfm_file_sector::range() {
   return sector_range;
}

int64_t vfm_file_sector::size() {
   return sector_range.size();
}

void vfm_file_sector::read(int64_t read_offset, int64_t read_length, uint8_t * buffer, int32_t buffer_offset) {
   std::fstream fs;
   fs.open(path, std::fstream::in | std::fstream::binary);
   fs.seekg(read_offset);
   fs.read((char*)buffer + buffer_offset, read_length);
}

void vfm_file_sector::deserialize(dargon::binary_reader & reader) {
   path = reader.read_null_terminated_string();
   
   auto offset = reader.read_int64();
   auto length = reader.read_int64();
   sector_range = vfm_sector_range(offset, length);
}

std::string vfm_file_sector::to_string() {
   std::stringstream ss;
   ss << "[vfm_file_sector " << path << " : [" + std::to_string(sector_range.start_inclusive) << ", " << std::to_string(sector_range.end_exclusive) << ") ]";
   return ss.str();
}