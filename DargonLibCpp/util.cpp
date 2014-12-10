#include "dlc_pch.hpp"
#include <clocale>
#include <codecvt>
#include <locale>
#include <string>
#include <sstream> 
#include <vector>
#include <Windows.h>
#include <TlHelp32.h>
#include "util.hpp"

using namespace dargon;

std::string GetFileName(const std::string& path)
{
   auto index = path.find_last_of("/\\") + 1;
   auto result = path.substr(index);
   std::cout << "path " << index << " " << result << " " << path << std::endl;
   return result;
}

HANDLE OpenMainThread()
{
   //Takes a snapshot of the threads of the current process id
   HANDLE hThreadSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
   DWORD currentProcessId = GetCurrentProcessId();
   DWORD currentThreadId = GetCurrentThreadId();

   HANDLE hEarliestCreationTimeThread = NULL;
   FILETIME earliestCreationTime;
   ZeroMemory(&earliestCreationTime, sizeof(earliestCreationTime));
   if (hThreadSnapshot != INVALID_HANDLE_VALUE)
   {
      THREADENTRY32 te;
      te.dwSize = sizeof(te);

      // Get the first thread from the snapshot
      if (Thread32First(hThreadSnapshot, &te))
      {
         do
         {
            //If the thread entry contains our owner process id (and thusly threadid, "cntUsage"
            //and dwSize). we can continue forwards.
            if (te.dwSize >= FIELD_OFFSET(THREADENTRY32, th32OwnerProcessID) + sizeof(te.th32OwnerProcessID))
            {    
               //Filter down so that we get suspend League of Legends, and that we don't get Dargon
               if(te.th32OwnerProcessID == currentProcessId && te.th32ThreadID != currentThreadId)
               {
                  //Get a handle to the given thread...
                  HANDLE hThread = OpenThread(THREAD_ALL_ACCESS, false, te.th32ThreadID);
                  std::cout << "THREAD CANDIDATE: pid" << te.th32OwnerProcessID << " tid" << te.th32ThreadID << " hThread" << hThread << std::endl ;

                  //Get how long this thread has lived for.  The main thread will have lived for the longest time
                  FILETIME creationTime, exitTime, kernelTime, userTime;
                  GetThreadTimes(hThread, &creationTime, &exitTime, &kernelTime, &userTime);

                  std::cout << " - creationTime: " << creationTime.dwHighDateTime << creationTime.dwLowDateTime << std::endl;

                  //                                        Returns 1 if the first file time is later than the other
                  if(hEarliestCreationTimeThread == NULL || CompareFileTime(&earliestCreationTime, &creationTime) == 1)
                  {
                     if(hEarliestCreationTimeThread != NULL) 
                        CloseHandle(hEarliestCreationTimeThread);
                     hEarliestCreationTimeThread = hThread;
                     earliestCreationTime = creationTime;
                  }
               }
            }
            te.dwSize = sizeof(te);
         } while (Thread32Next(hThreadSnapshot, &te));
      }
      return hEarliestCreationTimeThread;
   }
   return NULL;
}

HMODULE WaitForModuleHandle(const char* moduleName)
{
   std::cout << "Waiting for " << moduleName << " module load" << std::endl;
   HMODULE hModule;
   while ((hModule = GetModuleHandleA(moduleName)) == NULL) Sleep(10);
   std::cout << "The " << moduleName << " module loaded.  hModule: " << hModule << std::endl;
   return hModule;
}

std::vector<std::string> &split(const std::string &s, char delim, std::vector<std::string> &elems) {
   std::stringstream ss(s);
   std::string item;
   while (std::getline(ss, item, delim)) {
      elems.push_back(item);
   }
   return elems;
}


std::vector<std::string> split(const std::string &s, char delim) {
   std::vector<std::string> elems;
   split(s, delim, elems);
   return elems;
}

namespace dargon {
   std::string narrow(const std::wstring &s) {
      return std::wstring_convert<std::codecvt_utf8<wchar_t>, wchar_t>().to_bytes(s);
   }

   std::wstring wide(const std::string &s) {
      return std::wstring_convert<std::codecvt_utf8<wchar_t>, wchar_t>().from_bytes(s);
   }

   bool iequals(const std::string& a, const std::string& b) {
      auto length = a.length();
      if (length != b.length()) {
         return false;
      }
      for (size_t i = 0; i < length; i++) {
         if (tolower(a[i]) != tolower(b[i])) {
            return false;
         }
      }
      return true;
   }

   std::string join(const std::vector<std::string>& strings, const char* delimiter) {
      auto size = strings.size();
      if (size == 0) {
         return "";
      } else if (size == 1) {
         return strings[0];
      } else {
         std::stringstream ss;
         for (auto i = 0; i < size; i++) {
            if (i != 0) {
               ss << delimiter;
            }
            ss << strings[i];
         }
         return ss.str();
      }
   }

   bool contains(const std::vector<std::string>& collection, const char * string) {
      for (auto s : collection) {
         if (s.compare(string) == 0) {
            return true;
         }
      }
      return false;
   }

}