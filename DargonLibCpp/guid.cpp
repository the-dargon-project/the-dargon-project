#include "dlc_pch.hpp"
#include "guid.hpp"

namespace dargon {
   char nyble_to_hex(int nyble) {
      if (0 <= nyble && nyble <= 9) {
         return '0' + nyble;
      } else if (10 <= nyble && nyble <= 15) {
         return 'a' + (nyble - 10);
      } else {
         throw std::exception("Invalid argument; not a nyble.");
      }
   }

   int hex_to_nyble(char c) {
      if ('0' <= c && c <= '9') {
         return c - '0';
      } else if ('a' <= c && c <= 'f') {
         return 10 + (c - 'a');
      } else if ('A' <= c && c <= 'F') {
         return 10 + (c - 'A');
      } else {
         throw std::exception("Invalid argument; not a hex digit.");
      }
   }

   uint16_t flip_bytes(uint16_t value) {
      return ((value << 8) & 0xFF00) |
             ((value >> 8) & 0x00FF);
   }
   uint32_t flip_bytes(uint32_t value) {
      return ((value << 24) & 0xFF000000LU) |
             ((value << 8)  & 0x00FF0000LU) |
             ((value >> 8)  & 0x0000FF00LU) |
             ((value >> 24) & 0x000000FFLU);
   }
   uint64_t flip_bytes(uint64_t value) {
      return ((value << 56) & 0xFF00000000000000ULL) |
             ((value << 40) & 0x00FF000000000000ULL) |
             ((value << 24) & 0x0000FF0000000000ULL) |
             ((value << 8)  & 0x000000FF00000000ULL) |
             ((value >> 8)  & 0x00000000FF000000ULL) |
             ((value >> 24) & 0x0000000000FF0000ULL) |
             ((value >> 40) & 0x000000000000FF00ULL) |
             ((value >> 56) & 0x00000000000000FFULL);
   }

   guid guid::parse(const char* input_base64) {
      if (strlen(input_base64) != 2 * GUID_LENGTH) {
         throw std::exception("Failed to parse guid; incorrect length.");
      }

      uint8_t data[GUID_LENGTH];
      for (auto i = 0; i < GUID_LENGTH; i++) {
         auto upper = hex_to_nyble(input_base64[i * 2]);
         auto lower = hex_to_nyble(input_base64[i * 2 + 1]);
         data[i] = (upper << 4) | lower;
      }
      *(uint32_t*)data = flip_bytes(*(uint32_t*)data);
      *(uint16_t*)(data + 4) = flip_bytes(*(uint16_t*)(data + 4));
      *(uint16_t*)(data + 6) = flip_bytes(*(uint16_t*)(data + 6));

      guid result;
      memcpy(result.data, data, GUID_LENGTH);
      return result;
   }

   std::string guid::to_string() const {
      uint8_t data[GUID_LENGTH];
      memcpy(data, this->data, GUID_LENGTH);
      *(uint32_t*)data = flip_bytes(*(uint32_t*)data);
      *(uint16_t*)(data + 4) = flip_bytes(*(uint16_t*)(data + 4));
      *(uint16_t*)(data + 6) = flip_bytes(*(uint16_t*)(data + 6));

      char buffer[GUID_LENGTH * 2 + 1];
      for (auto i = 0; i < GUID_LENGTH; i++) {
         auto octet = data[i];
         auto lower = octet & 0x0F;
         auto upper = (octet >> 4) & 0x0F;
         buffer[i * 2 + 0] = nyble_to_hex(upper);
         buffer[i * 2 + 1] = nyble_to_hex(lower);
      }
      buffer[GUID_LENGTH * 2] = '\0';
      return std::string(buffer);
   }
}
