#include "stdafx.h"
#include "SystemState.hpp"
#include <ShlObj.h>
#include <iostream>
#include <sstream>
#include "util.hpp"

bool CheckFeatureToggle(std::wstring name) {
    bool result = false;
    WCHAR userHomePath[MAX_PATH];
    if (SUCCEEDED(::SHGetFolderPathW(NULL, CSIDL_PROFILE, NULL, 0, userHomePath))) {
       std::wstringstream ss;
       ss << userHomePath << L"/.dargon/configuration/system-state/" << name;

       std::fstream fs(ss.str().c_str(), std::fstream::in);
       if (fs.good()) {
          std::string token;
          fs >> token;
          if (!fs.fail() && dargon::iequals(token, "True")) {
             result = true;
          }
       }
       fs.close();
    }
    return result;
}
