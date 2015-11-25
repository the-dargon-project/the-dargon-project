#pragma once
#include <string>
#include <vector>

struct ICLRMetaHost;
struct ICLRRuntimeInfo;

namespace dargon {
   class clr_host {
      static ICLRMetaHost* metaHost;
      static ICLRRuntimeInfo* runtime;
      static ICLRRuntimeHost* runtimeHost;

   public:
      static void init(std::wstring version);
      static void load_assembly(std::wstring path);
   };

   class clr_utilities {
   public:
      static std::vector<std::wstring> enumerate_runtime_versions();
      static std::wstring pick_runtime_version();
   };
}