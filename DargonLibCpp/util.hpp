#include "dlc_pch.hpp"

#include <WinBase.h>
#include <string>
#include <vector>

#include "logger.hpp"
#include "file_logger.hpp"
#include "countdown_event.hpp"
#include "buffer_manager.hpp"
#include "unique_id_set.hpp"

std::string GetFileName(const std::string& filePath);

HANDLE OpenMainThread();
HMODULE WaitForModuleHandle(const char* moduleName);

// http://stackoverflow.com/questions/236129/how-to-split-a-string-in-c
std::vector<std::string> &split(const std::string &s, char delim, std::vector<std::string> &elems);
std::vector<std::string> split(const std::string &s, char delim);

namespace dargon {
   std::string narrow(const std::wstring &s);
   std::wstring wide(const std::string &s);

   bool iequals(const std::string& a, const std::string& b);
   std::string join(const std::vector<std::string>& strings, const char* delimiter);
   bool contains(const std::vector<std::string>& collection, const char* string);
}