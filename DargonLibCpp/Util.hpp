#include "dlc_pch.hpp"

#include <WinBase.h>
#include <string>
#include <vector>

#include "Util/ILogger.hpp"
#include "Util/Logger.hpp"

#include "Util/CountdownEvent.hpp"

#include "Util/BufferManager.hpp"
#include "Util/UniqueIdentificationSet.hpp"

std::string GetFileName(const std::string& filePath);

HANDLE OpenMainThread();

// http://stackoverflow.com/questions/236129/how-to-split-a-string-in-c
std::vector<std::string> &split(const std::string &s, char delim, std::vector<std::string> &elems);
std::vector<std::string> split(const std::string &s, char delim);