#pragma once

#include <memory>
#include "vfm_sector.hpp"
#include "io/IoProxy.hpp"

namespace dargon {
   class vfm_file_sector : public vfm_sector {
   public:
      static const dargon::guid kGuid;

   private:
      std::string path;
      int64_t offset;
      int64_t length;
      std::shared_ptr<dargon::IO::IoProxy> io_proxy;

   public:
      vfm_file_sector(std::shared_ptr<dargon::IO::IoProxy> io_proxy);

      virtual int64_t size() override;
      virtual void read(int64_t read_offset, int64_t read_length, uint8_t* buffer, int32_t buffer_offset) override;
      virtual void deserialize(dargon::binary_reader& reader) override;
      virtual std::string to_string() override;
   };
}