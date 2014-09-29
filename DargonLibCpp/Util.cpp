#include "dlc_pch.hpp"
#include <Windows.h>
#include <TlHelp32.h>
#include "Util.hpp"

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