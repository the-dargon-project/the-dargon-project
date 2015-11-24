#pragma once

#ifndef DLC_PCH_HPP
#define DLC_PCH_HPP

// Says we're building for Windows XP
//#ifdef _WIN32 
//#define _WIN32_WINNT 0x0501
//#endif

#ifdef _WIN32 
#define NOMINMAX
#include <Windows.h>

// Undefine some annoying preprocessor definitions Windows.h defines.  It's against our coding
// standard to use them (we want to explicitly use SendMessageW and SendMessageA, for example).
#undef SendMessage
#undef CreateProcess
#endif

#endif