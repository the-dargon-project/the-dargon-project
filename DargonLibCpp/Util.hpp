#include "dlc_pch.hpp"

#include <WinBase.h>
#include <string>

#include "Util/ILogger.hpp"
#include "Util/Logger.hpp"

#include "Util/CountdownEvent.hpp"

#include "Util/BufferManager.hpp"
#include "Util/UniqueIdentificationSet.hpp"

std::string GetFileName(const std::string& filePath);

HANDLE OpenMainThread();