#include "dlc_pch.hpp"
#include <fstream>
#include <sstream>
#include "binary_reader.hpp"
#include "vfm_file_sector.hpp"

using namespace dargon;

const dargon::guid vfm_file_sector::kGuid(guid::parse("5DB2B4C239AE4629988ACFFFCE89F230"));

vfm_file_sector::vfm_file_sector(std::shared_ptr<dargon::IO::IoProxy> io_proxy) : io_proxy(io_proxy) {

}

vfm_sector_range vfm_file_sector::range() {
   return sector_range;
}

int64_t vfm_file_sector::size() {
   return sector_range.size();
}

void vfm_file_sector::read(int64_t read_offset, int64_t read_length, uint8_t * buffer, int32_t buffer_offset) {
   auto file = io_proxy->CreateFileA(path.c_str(), GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

   if (file == INVALID_HANDLE_VALUE) {
      MessageBoxA(NULL, ("VFM Failed to open file " + path).c_str(), "", MB_OK);
   }

   LARGE_INTEGER li_read_offset;
   li_read_offset.QuadPart = read_offset;
   io_proxy->SetFilePointerEx(file, li_read_offset, nullptr, SEEK_SET);

   DWORD bytes_to_read = read_length;
   uint8_t* currentBufferPointer = buffer + buffer_offset;
   while (bytes_to_read > 0) {
      DWORD bytes_read = 0;
      io_proxy->ReadFile(file, currentBufferPointer, bytes_to_read, &bytes_read, nullptr);
      bytes_to_read -= bytes_read;
      currentBufferPointer += bytes_read;
   }

   io_proxy->CloseHandle(file);
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