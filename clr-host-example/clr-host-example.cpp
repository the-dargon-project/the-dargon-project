// clr-host-example.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <clr_host.hpp>

int main() {
   dargon::clr_host::init(dargon::clr_utilities::pick_runtime_version());
   auto path = L"../../../clr-hosted-egg-example/bin/Debug/clr-hosted-egg-example.dll";
   dargon::clr_host::load_assembly(path, L"Hello from C++!");
   return 0;
}
