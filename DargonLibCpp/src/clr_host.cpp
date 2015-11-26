#include "dlc_pch.hpp"
#include <algorithm>
#include <iostream>
#include <metahost.h>
#include "clr_host.hpp"

ICLRMetaHost* dargon::clr_host::metaHost;
ICLRRuntimeInfo* dargon::clr_host::runtime;
ICLRRuntimeHost* dargon::clr_host::runtimeHost;;

void dargon::clr_host::init(std::wstring version) {
   std::wcout << "Initialize CLR " << version << std::endl;
   HRESULT hr;
   hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&metaHost));
   std::cout << "CLRCreateInstance -> " << hr << std::endl;
   hr = metaHost->GetRuntime(version.c_str(), IID_PPV_ARGS(&runtime));
   std::cout << "GetRuntime -> " << hr << std::endl;
   hr = runtime->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&runtimeHost));
   std::cout << "GetInterface -> " << hr << std::endl;
}

void dargon::clr_host::load_assembly(std::wstring path, std::wstring args) {
   HRESULT hr;
   hr = runtimeHost->Start();
   std::cout << "Runtime.Start -> " << hr << std::endl;
   DWORD returnValue;
   hr = runtimeHost->ExecuteInDefaultAppDomain(
      path.c_str(),
      L"TrinketEntryPoint",
      L"TrinketMain",
      args.c_str(),
      &returnValue);
   std::cout << "execute return code: " << returnValue << std::endl;
   std::cout << "execute hresult: " << std::hex << hr << std::endl;
}

std::wstring dargon::clr_utilities::pick_runtime_version() {
   auto versions = enumerate_runtime_versions();
   std::sort(versions.begin(), versions.end());
   return versions.at(versions.size() - 1);
}

std::vector<std::wstring> dargon::clr_utilities::enumerate_runtime_versions() {
   std::vector<std::wstring> versions;
   HRESULT hr;
   ICLRMetaHost* clrMetaHost;
   hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&clrMetaHost));
   IEnumUnknown* runtimes;
   hr = clrMetaHost->EnumerateInstalledRuntimes(&runtimes);
   IUnknown* runtime;
   while (runtimes->Next(1, &runtime, nullptr) == S_OK) {
      ICLRRuntimeInfo* runtimeInfo;
      hr = runtime->QueryInterface(IID_PPV_ARGS(&runtimeInfo));
      DWORD versionStringLength;
      hr = runtimeInfo->GetVersionString(nullptr, &versionStringLength);
      std::wstring versionString(versionStringLength, L'\0');
      hr = runtimeInfo->GetVersionString(const_cast<LPWSTR>(versionString.c_str()), &versionStringLength);
      runtimeInfo->Release();
      runtime->Release();
      versions.emplace_back(versionString);
   }
   runtimes->Release();
   clrMetaHost->Release();
   return versions;
}
