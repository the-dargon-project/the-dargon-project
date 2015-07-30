#pragma once

#include <string>
#include "base.hpp"
#include "util.hpp"

namespace dargon {
   const int GUID_LENGTH = 16;
   
   char nyble_to_hex(int nyble);
   int hex_to_nyble(char c);

   uint16_t flip_bytes(uint16_t value);
   uint32_t flip_bytes(uint32_t value);
   uint64_t flip_bytes(uint64_t value);

   struct guid {
      uint8_t data[GUID_LENGTH];

      guid() {
         memset(data, 0, GUID_LENGTH);
      }

      guid(const guid& value) {
         memcpy(data, value.data, GUID_LENGTH);
      }

      static guid parse(const char* input_base64);

      std::string to_string() const;

      bool operator==(const guid &other) const {
         return memcmp(data, other.data, GUID_LENGTH) == 0;
      }

      bool operator!=(const guid &other) const {
         return !(*this == other);
      }
   };
}


namespace std {
   template <> struct hash<dargon::guid> {
      size_t operator()(const dargon::guid& value) const {
         size_t hash = 0x47F09ACEUL;
         hash ^= ((uint32_t*)value.data)[0];
         hash ^= ((uint32_t*)value.data)[1];
         hash ^= ((uint32_t*)value.data)[2];
         hash ^= ((uint32_t*)value.data)[3];
         return hash;
      }
   };
}