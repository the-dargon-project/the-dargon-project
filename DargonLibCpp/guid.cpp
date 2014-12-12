#include "dlc_pch.hpp"
#include "guid.hpp"

namespace dargon {
   char nyble_to_hex(int nyble) {
      if (0 <= nyble && nyble <= 9) {
         return '0' + nyble;
      } else if (10 <= nyble && nyble <= 15) {
         return 'a' + nyble;
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

   guid guid::parse(const char* input_base64) {
      if (strlen(input_base64) != GUID_LENGTH) {
         throw std::exception("Failed to parse guid; incorrect length.");
      }

      guid result;
      for (auto i = 0; i < GUID_LENGTH; i++) {
         auto upper = hex_to_nyble(input_base64[i * 2]);
         auto lower = hex_to_nyble(input_base64[i * 2 + 1]);
         result.data[i] = (upper << 4) | lower;
      }
      return result;
   }

   std::string guid::to_string() {
      char buffer[GUID_LENGTH * 2 + 1];
      for (auto i = 0; i < GUID_LENGTH; i++) {
         auto octet = data[i];
         auto lower = octet & 0x0F;
         auto upper = (octet >> 4) & 0x0F;
         buffer[i * 2 + 0] = nyble_to_hex(upper);
         buffer[i * 2 + 1] = nyble_to_hex(lower);
      }
      return std::string(buffer);
   }
}
