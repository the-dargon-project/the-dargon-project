#include "stdafx.h"
#include <algorithm>

#include "base.hpp"
#include "util.hpp"

#include "Configuration.hpp"
using namespace dargon;

const std::string Configuration::EnableCommandListFlag = "--enable-dim-command-list";
const std::string Configuration::EnableFileSystemHooksFlag = "--enable-filesystem-hooks";
const std::string Configuration::LaunchSuspendedKey = "launchsuspended";

std::shared_ptr<Configuration> Configuration::Parse(flags_t flags, property_pairs_t property_pairs) {
   properties_t properties;
   for (auto it = property_pairs.begin(); it != property_pairs.end(); it++) {
      properties.insert(std::make_pair(it->first, it->second));
   }
   return Parse(flags, properties);
}

std::shared_ptr<Configuration> Configuration::Parse(flags_t flags, properties_t properties) {
   auto configuration = std::make_shared<Configuration>(flags, properties);
   configuration->Initialize();
   return configuration;
}

void Configuration::Initialize() {

}

bool Configuration::IsFlagSet(const std::string& flag) {
   return IsFlagSet(flag.c_str());
}

bool Configuration::IsFlagSet(const char* flag) {
   return dargon::contains(flags, flag);
}

std::string Configuration::GetProperty(const std::string& key) {
   auto match = properties.find(key);
   if (match != properties.end()) {
      return match->second;
   } else {
      return "";
   }
}