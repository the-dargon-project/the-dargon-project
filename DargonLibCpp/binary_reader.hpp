#pragma once

#include <string>
#include <memory>
#include <vector>
#include "Base.hpp"
#include "noncopyable.hpp"

namespace dargon {
#define simple_read_impl(integer_type) \
   void read_##integer_type(integer_type##_t* dest) { \
      auto next = current + sizeof(integer_type##_t); \
      if (next > origin + length) { \
         throw_read_past_eof(); \
      } else { \
         *dest = *(integer_type##_t*)current; \
         current = next; \
      } \
   } \
   integer_type##_t read_##integer_type() { \
      auto next = current + sizeof(integer_type##_t); \
      if (next > origin + length) { \
         throw_read_past_eof(); \
         return 0; \
      } else { \
         auto result = *(integer_type##_t*)current; \
         current = next; \
         return result; \
      } \
   }
         

   class binary_reader : dargon::noncopyable {
      const uint8_t* origin;
      const uint8_t* current;
      int32_t length;

   private:
      inline void throw_read_past_eof() {
         throw "attempted to read past end of stream";
      }

   public:
      binary_reader(const void* buffer, int32_t length) : origin((const uint8_t*)buffer), current((const uint8_t*)buffer), length(length) { };

      simple_read_impl(int8)
      simple_read_impl(int16)
      simple_read_impl(int32)
      simple_read_impl(int64)

      simple_read_impl(uint8)
      simple_read_impl(uint16)
      simple_read_impl(uint32)
      simple_read_impl(uint64)

      void read_bytes(void* buffer, int32_t count) {
         if (current + count > origin + length) {
            throw_read_past_eof();
         }
         memcpy(buffer, current, count);
         current += count;
      }

      void read_null_terminated_string(std::string* result) {
         *result = read_null_terminated_string();
      }

      std::string read_null_terminated_string() {
         std::vector<char> chars;
         char c;
         while ((c = *(current++)) != 0) {
            chars.push_back(c);
            if (current > origin + length) {
               throw_read_past_eof();
            }
         }
         return std::string(chars.begin(), chars.end());
      }

      std::string read_tiny_text() {
         uint32_t length = read_uint8();
         auto data = std::make_unique<uint8_t[]>(length + 1); // +1 for null terminator
         read_bytes(data.get(), length);
         data[length] = 0;
         return std::string((const char*)data.get(), length);
      }

      std::string read_text() {
         uint32_t length = read_uint16();
         auto data = std::make_unique<uint8_t[]>(length + 1); // +1 for null terminator
         read_bytes(data.get(), length);
         data[length] = 0;
         return std::string((const char*)data.get(), length);
      }

      std::string read_long_text() {
         uint32_t length = read_uint32();
         auto data = std::make_unique<uint8_t[]>(length + 1); // +1 for null terminator
         read_bytes(data.get(), length);
         data[length] = 0;
         return std::string((const char*)data.get(), length);
      }

      size_t available() {
         return (origin + length) - current;
      }
   };
}