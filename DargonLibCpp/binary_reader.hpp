#pragma once

#include <string>
#include <memory>
#include <vector>
#include <iostream> // cout for debugging
#include "base.hpp"
#include "noncopyable.hpp"
#include "memory_stream.hpp"

namespace dargon {
#define simple_read_impl(integer_type) \
   void read_##integer_type(integer_type##_t* dest) { \
      read_bytes(dest, sizeof(integer_type##_t)); \
   } \
   integer_type##_t read_##integer_type() { \
      integer_type##_t result; \
      read_bytes(&result, sizeof(result)); \
      return result; \
   }
         
   class binary_reader : dargon::noncopyable {
      std::shared_ptr<std::istream> stream;

   private:
      inline void throw_read_past_eof() {
         throw std::exception("attempted to read past end of stream");
      }

   public:
      binary_reader(const void* buffer, std::size_t length) : binary_reader(std::make_shared<memory_stream>((char*)buffer, length)) { }
      binary_reader(std::shared_ptr<std::istream> stream) : stream(stream) { }

      void read_bytes(void* buffer, int32_t count) {
         stream->read((char*)buffer, count);

         if (stream->eof()) {
            throw_read_past_eof();
         }
      }

      simple_read_impl(int8)
      simple_read_impl(int16)
      simple_read_impl(int32)
      simple_read_impl(int64)

      simple_read_impl(uint8)
      simple_read_impl(uint16)
      simple_read_impl(uint32)
      simple_read_impl(uint64)

      void read_null_terminated_string(std::string* result) {
         *result = read_null_terminated_string();
      }

      std::string read_null_terminated_string() {
         std::vector<char> chars;
         char c;
         while ((c = read_int8()) != 0) {
            chars.push_back(c);
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
         if (stream->peek() == EOF) {
            return 0;
         } else {
            return 1;
         }
      }
   };
}