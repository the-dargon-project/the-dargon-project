#pragma once

#ifndef DLC_PCH_HPP
#define DLC_PCH_HPP

// Says we're building for Windows XP
#ifdef _WIN32 
#define _WIN32_WINNT 0x0501
#endif

// Stops boost bind from making placeholders global
#define BOOST_BIND_NO_PLACEHOLDERS
#include <boost/iostreams/concepts.hpp>
#include <boost/iostreams/device/file.hpp>

// boost.asio must be included before windows.h (well, winsock.h, which windows.h includes)
#define _WINSOCK_DEPRECATED_NO_WARNINGS
#include <boost/asio.hpp>

#ifdef _WIN32 
#include <Windows.h>

// Undefine some annoying preprocessor definitions Windows.h defines.  It's against our coding
// standard to use them (we want to explicitly use SendMessageW and SendMessageA, for example).
#undef SendMessage
#undef CreateProcess
#endif

#endif