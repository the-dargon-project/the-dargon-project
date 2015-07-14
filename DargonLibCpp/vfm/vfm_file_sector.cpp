#include "dlc_pch.hpp"
#include <fstream>
#include <sstream>
#include "binary_reader.hpp"
#include "vfm_file_sector.hpp"

using namespace dargon;

const dargon::guid vfm_file_sector::kGuid(guid::parse("5DB2B4C239AE4629988ACFFFCE89F230"));

vfm_file_sector::vfm_file_sector(std::shared_ptr<dargon::IO::IoProxy> io_proxy) : io_proxy(io_proxy), offset(0), length(0) {

}

int64_t vfm_file_sector::size() {
   return length;
}

void vfm_file_sector::read(int64_t read_offset, int64_t read_length, uint8_t * buffer, int32_t buffer_offset) {
   if (buffer_offset > 0) {
      return read(read_offset, read_length, buffer + buffer_offset, 0);
   }

   auto file = io_proxy->CreateFileA(path.c_str(), GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

   if (file == INVALID_HANDLE_VALUE) {
      std::cout << "VFM FAILED TO OPEN FILE " << path.c_str() << ":(" << std::endl;
      MessageBoxA(NULL, ("VFM Failed to open file " + path).c_str(), "", MB_OK);
   }

   LARGE_INTEGER li_read_offset;
   li_read_offset.QuadPart = read_offset + offset;
   io_proxy->SetFilePointerEx(file, li_read_offset, nullptr, FILE_BEGIN);

   int64_t bytes_remaining = read_length;
   int64_t total_bytes_read = 0;
   while (bytes_remaining > 0) {
      DWORD bytes_read = 0;
      io_proxy->ReadFile(file, buffer + total_bytes_read, bytes_remaining, &bytes_read, nullptr);
      bytes_remaining -= bytes_read;
      total_bytes_read += bytes_read;
   }

   io_proxy->CloseHandle(file);
}

void vfm_file_sector::deserialize(dargon::binary_reader & reader) {
   path = reader.read_null_terminated_string();
   
   offset = reader.read_int64();
   length = reader.read_int64();
}

std::string vfm_file_sector::to_string() {
   std::stringstream ss;
   ss << "[vfm_file_sector " << path << " off = " + std::to_string(offset) << ", len = " << std::to_string(length) << " ]";
   return ss.str();
}