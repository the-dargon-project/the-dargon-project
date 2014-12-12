#pragma once
#include <streambuf>
#include <istream>
#include "base.hpp"

namespace dargon {
   class memorybuf : public std::streambuf {
      bool free_on_destruct;

   public:
      memorybuf(dargon::blob* blob) : memorybuf((char*)blob->data, blob->size, false) { }
      memorybuf(char* buffer, std::size_t length) : memorybuf(buffer, length, false) { }
      memorybuf(char* buffer, std::size_t length, bool free_on_destruct) : free_on_destruct(free_on_destruct) {
         setg(buffer, buffer, buffer + length);
      }

      ~memorybuf() {
         if (free_on_destruct) {
            delete eback();
         }
      }
   };

   class memory_stream : public memorybuf, public std::istream {
   public:
      memory_stream(dargon::blob* blob) : memory_stream((char*)blob->data, blob->size, false) { }
      memory_stream(char* buffer, std::size_t length) : memory_stream(buffer, length, false) { }
      memory_stream(char* buffer, std::size_t length, bool free_on_distruct)
         : memorybuf(buffer, length, free_on_distruct), 
           std::istream(static_cast<std::streambuf*>(this)) { }
   };
}