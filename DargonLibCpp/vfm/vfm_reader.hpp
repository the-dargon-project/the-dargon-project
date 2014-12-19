#pragma once

#include <memory>
#include <vector>

#include "base.hpp"
#include "binary_reader.hpp"

#include "vfm_file.hpp"
#include "vfm_sector.hpp"
#include "vfm_sector_factory.hpp"

namespace dargon {
   const uint32_t vfm_sector_collection_magic = 0x534D4656U;

   class vfm_reader {
      std::shared_ptr<vfm_sector_factory> sector_factory;

   public:
      vfm_reader(std::shared_ptr<vfm_sector_factory> sector_factory) 
      : sector_factory(sector_factory) { }

      std::shared_ptr<vfm_file> load(dargon::binary_reader& reader) {
         auto magic = reader.read_uint32();
         if (magic != vfm_sector_collection_magic) {
            std::cout << "sector collection magic mismatch. got " << std::hex << magic << " but expected " << vfm_sector_collection_magic << std::endl;
            throw std::exception("sector collection magic mismatch.");
         }
         auto result = std::make_shared<vfm_file>();
         auto sector_count = reader.read_uint32();
         for (auto i = 0; i < sector_count; i++) {
            auto guid = reader.read_guid();
            auto startInclusive = reader.read_int64();
            auto endExclusive = reader.read_int64();
            auto sector = sector_factory->create(guid);
            sector->deserialize(reader);
            result->assign_sector(sector);
         }
         return result;
      }
   };
}