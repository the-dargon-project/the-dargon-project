#include "dlc_pch.hpp"
#include <thread>
#include <sstream>
#include <iomanip>
#include "file_logger.hpp"

#if WIN32
#include <Windows.h>
#endif

using namespace dargon;
file_logger* file_logger::s_instance = nullptr;
void file_logger::initialize(std::string fileName)
{
   s_instance = new file_logger(fileName);
}

/// <summary>
/// Initializes our file_logger class, which can be used for outputting to a location
/// </summary>
/// <param name="fileName">The path to the file which we are outputting to.</param>
file_logger::file_logger(std::string fileName)
   : m_file_loggerFilter(LL_VERBOSE), m_indentationCount(0), m_outputStream(std::ofstream(fileName, std::ios::out | std::ios::binary))
{
   Log(LL_INFO, [](std::ostream& os){ os << "file_logger Initialized." << std::endl; });
}

void file_logger::Log(UINT32 file_loggerLevel, LoggingFunction loggingFunction)
{
   if(file_loggerLevel < m_file_loggerFilter)
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
   
   if(file_loggerLevel == LL_VERBOSE)
       stringStream << "VERBOSE| ";
   else if(file_loggerLevel == LL_INFO)
       stringStream << "   INFO| ";
   else if(file_loggerLevel == LL_NOTICE)
       stringStream << " NOTICE| ";
   else if(file_loggerLevel == LL_WARN)
       stringStream << "   WARN| ";
   else if(file_loggerLevel == LL_ERROR)
       stringStream << "  ERROR| ";
   else if(file_loggerLevel == LL_ALWAYS)
       stringStream << " ALWAYS| ";
   else 
       stringStream << "-------| ";
   
   loggingFunction(stringStream);
   auto str = stringStream.str();

   std::cout << str;
   m_outputStream << str;
   m_outputStream.flush();
}