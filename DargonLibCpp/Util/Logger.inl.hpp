#include "Logger.hpp"
void Dargon::Util::Logger::L(UINT32 loggerLevel, DoLog logger)
{
   if(s_instance != nullptr)
      s_instance->Log(loggerLevel, logger);
}
void Dargon::Util::Logger::SL(UINT32 loggerLevel, DoLog logger)
{
   if(s_instance != nullptr)
      s_instance->Log(loggerLevel, logger);
}
void Dargon::Util::Logger::SNL(UINT32 loggerLevel, DoLog logger)
{
   if(s_instance != nullptr)
      s_instance->Log(loggerLevel, logger);
}