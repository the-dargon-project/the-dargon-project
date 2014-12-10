#pragma once
#include <memory> // shared_ptr
#include <string> // string
#include <unordered_map> // unordered_map
#include <utility> // std::pair
#include <vector> // vector

namespace dargon {
   class Configuration {
      typedef std::vector<std::string> flags_t;
      typedef std::unordered_map<std::string, std::string> properties_t;
      typedef std::vector<std::pair<std::string, std::string>> property_pairs_t;

   public:
      static const std::string EnableTaskListFlag;

      static std::shared_ptr<Configuration> Parse(flags_t flags, property_pairs_t properties);
      static std::shared_ptr<Configuration> Parse(flags_t flags, properties_t properties);

      Configuration(flags_t flags, properties_t properties) : flags(flags), properties(properties) {}

   private:
      flags_t flags;
      properties_t properties;

      void Initialize();

   public:
      bool IsFlagSet(const std::string& flag);
      bool IsFlagSet(const char* flag);
      std::string GetProperty(const std::string& key);
   };
}