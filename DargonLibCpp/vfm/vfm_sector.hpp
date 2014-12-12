#pragma once
#include "util.hpp";
#include "noncopyable.hpp";
#include "binary_reader.hpp";

namespace dargon {
   struct vfm_sector_range {
      int64_t start_inclusive;
      int64_t end_exclusive;

      vfm_sector_range() : vfm_sector_range(0, 0) { }
      vfm_sector_range(int64_t start_inclusive, int64_t end_exclusive) : start_inclusive(start_inclusive), end_exclusive(end_exclusive) { }
      long size() { return end_exclusive - start_inclusive; }

      bool intersects(const vfm_sector_range& range) { return !((start_inclusive >= range.end_exclusive) || (range.start_inclusive >= end_exclusive)); }
      bool fully_contains(const vfm_sector_range& range) { return start_inclusive <= range.start_inclusive && range.end_exclusive <= end_exclusive; }
      bool contains(long x) { return start_inclusive <= x && x < end_exclusive; }
   };

   class vfm_sector : dargon::noncopyable {
   public:
      virtual vfm_sector_range range() = 0;

      virtual int64_t size() = 0;
      virtual void read(int64_t read_offset, int64_t read_length, uint8_t* buffer, int32_t bufferOffset) = 0;
      virtual void deserialize(dargon::binary_reader& reader) = 0;
      virtual std::string to_string() = 0;
   };
}