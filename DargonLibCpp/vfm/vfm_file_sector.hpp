#pragma once

#include "vfm_sector.hpp"

namespace dargon {
   class vfm_file_sector : public vfm_sector {
   public:
      static const dargon::guid kGuid;

   private:
      std::string path;
      vfm_sector_range sector_range;

   public:
      vfm_file_sector();

      virtual vfm_sector_range range() override;
      virtual int64_t size() override;
      virtual void read(int64_t read_offset, int64_t read_length, uint8_t* buffer, int32_t buffer_offset) override;
      virtual void deserialize(dargon::binary_reader& reader) override;
      virtual std::string to_string() override;
   };
}