#include "../dlc_pch.hpp"
#include <thread>
#include <sstream>
#include <iomanip>
#include "Logger.hpp"

#if WIN32
#include <Windows.h>
#endif

using namespace dargon::Util;
Logger* Logger::s_instance = nullptr;
void Logger::Initialize(std::string fileName)
{
   s_instance = new Logger(fileName);
}

/// <summary>
/// Initializes our Logger class, which can be used for outputting to a location
/// </summary>
/// <param name="fileName">The path to the file which we are outputting to.</param>
Logger::Logger(std::string fileName)
   : m_loggerFilter(LL_VERBOSE), m_indentationCount(0), m_outputStream(std::ofstream(fileName, std::ios::out | std::ios::binary))
{
   Log(LL_INFO, [](std::ostream& os){ os << "Logger Initialized." << std::endl; });
}

void Logger::Log(UINT32 loggerLevel, LoggingFunction loggingFunction)
{
   if(loggerLevel < m_loggerFilter)
      return;

   std::stringstream stringStream;
#if WIN32
   SYSTEMTIME systemTime;
   GetLocalTime(&systemTime);

   static const char* DaysOfWeek[] = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
   static const char* MonthsInYear[] = { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
   stringStream << std::setfill('0') 
                << DaysOfWeek[systemTime.wDayOfWeek] << ' ' 
                << MonthsInYear[systemTime.wMonth] << ' '
                << std::setw(2) << systemTime.wDay << ' '
                << std::setw(2) << systemTime.wHour << ':'
                << std::setw(2) << systemTime.wMinute << ':'
                << std::setw(2) << systemTime.wSecond << ':'
                << std::setw(3) << systemTime.wMilliseconds << ' '
                << std::setw(4) << systemTime.wYear << '|'
                << std::setw(4) << std::hex << std::this_thread::get_id() << std::dec << "|";
#else
   UINT32 a = "Don't compile";
   #pragma message "Warning: Using localtime is unsafe for multithreaded environment!"
#endif
   
   if(loggerLevel == LL_VERBOSE)
       stringStream << "VERBOSE| ";
   else if(loggerLevel == LL_INFO)
       stringStream << "   INFO| ";
   else if(loggerLevel == LL_NOTICE)
       stringStream << " NOTICE| ";
   else if(loggerLevel == LL_WARN)
       stringStream << "   WARN| ";
   else if(loggerLevel == LL_ERROR)
       stringStream << "  ERROR| ";
   else if(loggerLevel == LL_ALWAYS)
       stringStream << " ALWAYS| ";
   else 
       stringStream << "-------| ";
   
   loggingFunction(stringStream);
   auto str = stringStream.str();

   std::cout << str;
   m_outputStream << str;
   m_outputStream.flush();
}