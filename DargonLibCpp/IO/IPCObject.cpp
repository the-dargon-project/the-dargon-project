#include "../dlc_pch.hpp"
#include <iostream>
#include <memory>
#include <sstream>
#include <string>

#ifdef WIN32
#include <Windows.h>
#endif

#include "../Util.hpp"
#include "IPCObject.hpp"

using namespace dargon::IO;
using dargon::file_logger;

IPCObject::IPCObject(std::shared_ptr<dargon::IO::IoProxy> ioProxy)
   : m_lastError(0), m_pipeHandle(0), ioProxy(ioProxy)
{
}

bool IPCObject::Open(IN const std::string& pipeName, 
                     IN dargon::IO::FileAccess accessMode,
                     IN dargon::IO::FileShare shareMode,
                     IN bool writesBuffered) 
{
#ifdef WIN32
   std::string pipePath = "\\\\.\\pipe\\" + pipeName;
   m_pipeHandle = ioProxy->CreateFileA(
      pipePath.c_str(),
      GENERIC_READ | GENERIC_WRITE, 
      0,
      NULL, 
      OPEN_EXISTING, 
      FILE_FLAG_WRITE_THROUGH | FILE_FLAG_OVERLAPPED,
      0
   );
   
   if(m_pipeHandle == INVALID_HANDLE_VALUE)
   {
      m_lastError = ::GetLastError();
      return false;
   }

   std::stringstream ss; ss << "Named Pipe Handle " << m_pipeHandle;
   return true;
#endif
}

UINT32 IPCObject::GetLastError()
{
   return m_lastError;
}

void IPCObject::Close()
{
}

//-------------------------------------------------------------------------------------------------
// IO Read/Write Operations
//-------------------------------------------------------------------------------------------------
dargon::Blob* IPCObject::ReadBytes(IN uint32_t numBytes)
{
#ifdef WIN32
   return nullptr;
   //BYTE* data = new BYTE[numBytes];
   //DWORD bytesRead;
   //if(!ReadFile(m_pipeHandle, data, numBytes, &bytesRead, nullptr))
   //{
   //   m_lastError = GetLastError();
   //   delete data;
   //   return nullptr;
   //}
   //else
   //{
   //   return new Blob(numBytes, data);
   //}
#endif
}

bool IPCObject::ReadBytes(OUT             void* buffer, 
                          IN              uint32_t numBytes, 
                          OUT OPTIONAL    uint32_t* paramBytesRead)
{
   file_logger::L(LL_VERBOSE, [=](std::ostream& os){ os << "Read: buffer " << buffer << " numBytes " << numBytes << std::endl; });

   static_assert(sizeof(DWORD) == sizeof(uint32_t), "DWORD size isn't same as UINT32 size!");
   if(numBytes == 0) return true;
   if(paramBytesRead != nullptr) *paramBytesRead = numBytes;

#ifdef WIN32
   OVERLAPPED overlapped;
   UINT32 totalBytesRead = 0;
   UINT32 totalBytesToRead = numBytes;

   do
   {
      UINT32 totalBytesToTransfer = totalBytesToRead - totalBytesRead;

      ZeroMemory(&overlapped, sizeof(overlapped));
      overlapped.hEvent = ioProxy->CreateEventA(NULL, TRUE, FALSE, NULL);

      BOOL readFileResult;
      DWORD readFileBytesRead = 0;
      if (!(readFileResult = ioProxy->ReadFile(m_pipeHandle, (BYTE*)buffer + totalBytesRead, totalBytesToTransfer, &readFileBytesRead, &overlapped)))
      {
         m_lastError = ::GetLastError();
         if(m_lastError != ERROR_IO_PENDING)
         {
            file_logger::SNL(LL_ERROR, [=](std::ostream& os) { os << "Read byte failed with error " << m_lastError << std::endl; });//; read " << bytesRead << " of " << numBytes << " remaining bytes." << std::endl; });
            ioProxy->CloseHandle(overlapped.hEvent);
            return false;
         }
         else 
         {
            //std::cout << "ReadBytes: Error code ERROR_IO_PENDING is okay " << std::endl;
         }
      }
      
      DWORD bytesTransferred = 0;
      if(readFileResult) //Only wait for overlapped if readfile returned FALSE (else, it was a synchronous call)
         bytesTransferred = readFileBytesRead;
      else
      {
         //std::cout << "ReadBytes: Waiting for single object" << std::endl;
         DWORD waitResult = WaitForSingleObject(overlapped.hEvent, INFINITE);
         if(waitResult != WAIT_OBJECT_0)
            file_logger::SNL(LL_ERROR, [=](std::ostream& os){ os << "IPC Object Read Wait for Single Object returned " << std::hex << waitResult << std::endl; });
         
         //std::cout << "ReadBytes: Getting overlapped result" << std::endl;
         if(!GetOverlappedResult(m_pipeHandle, &overlapped, &bytesTransferred, TRUE))
         {
            m_lastError = ::GetLastError();
            file_logger::SNL(LL_ERROR, [=](std::ostream& os){ os << "Get Overlapped IO Result failed with error " << m_lastError << std::endl; });
            ioProxy->CloseHandle(overlapped.hEvent);
            return false;
         }
      }
      ioProxy->CloseHandle(overlapped.hEvent);
      //std::cout << "Bytes Transferred " << bytesTransferred << " Is Complete? " << HasOverlappedIoCompleted(&overlapped) 
      //          << " buffer[0]: " << std::hex << (int)*(BYTE*)buffer << "(" << std::dec << (int)*(BYTE*)buffer << ")" << std::endl;

      totalBytesRead += bytesTransferred;
   } while(totalBytesRead < totalBytesToRead);

   //for(int i = 0; i < totalBytesToRead; i++)
   //{
   //   int value = *((BYTE*)buffer + i);
   //   file_logger::L(LL_VERBOSE, [=](std::ostream& os){ os << "R " << i << ": " << std::hex << std::setw(2) << value << std::dec << " (" << value << ") '" << (char)value << "'" << std::endl; });
   //}

      //file_logger::SNL(LL_VERBOSE, [=](std::ostream& os){ 
      //   os << "Read Bytes: ";
      //   if(bytesRead > 0)
      //   {
      //      os << (int)*((BYTE*)buffer);
      //      for(UINT32 n = 1; n < bytesRead; n++)
      //         os << ", " << (int)*((BYTE*)buffer + n); 
      //   }
      //   else
      //   {
      //      os << "[none]";
      //   }
      //   os << std::endl;
      //});

      //buffer = (BYTE*)buffer + bytesRead;
      //numBytes -= bytesRead;
      //file_logger::SNL(LL_VERBOSE, [=](std::ostream& os) { os << "Read byte count " << bytesRead << ", " << numBytes << " remaining" << std::endl; });
   //}
   return true;
#endif
}

bool IPCObject::Write(IN const void* buffer, 
                      IN uint32_t numBytes)
{
   return Write(buffer, 0, numBytes, nullptr);
}

bool IPCObject::Write(IN           const void* buffer, 
                      IN           uint32_t offset, 
                      IN           uint32_t numBytes,
                      OUT OPTIONAL uint32_t* bytesWritten)
{
   if(numBytes == 0) {
      if(bytesWritten)
         *bytesWritten = 0;
      return true;
   }

#ifdef WIN32
   static_assert(sizeof(DWORD) == sizeof(uint32_t), "DWORD size isn't same as UINT32 size!");

   OVERLAPPED overlapped;
   ZeroMemory(&overlapped, sizeof(OVERLAPPED));
   overlapped.hEvent = ioProxy->CreateEventW(NULL, FALSE, FALSE, NULL);
   
   bool success = true;
   if(!ioProxy->WriteFile(m_pipeHandle, (uint8_t*)buffer + offset, numBytes, nullptr, &overlapped)) {
      auto lastError = ::GetLastError();
      if (lastError != ERROR_IO_PENDING) {
         std::cout << "IPC Write File error " << m_lastError << std::endl;
         //file_logger::SNL(LL_ERROR, [this](std::ostream& os){ os << "IPC Write File error " << m_lastError << std::endl; });
         success = false;
      } else { 
         DWORD waitResult = WaitForSingleObject(overlapped.hEvent, INFINITE);
         if (waitResult != WAIT_OBJECT_0) {
            std::cout << "IPC Object Write Wait for Single Object returned " << std::hex << waitResult << std::endl;
            //file_logger::SNL(LL_ERROR, [=](std::ostream& os) { os << "IPC Object Write Wait for Single Object returned " << std::hex << waitResult << std::endl; });
         }
         DWORD bytesTransferred = 0;
         if (!GetOverlappedResult(m_pipeHandle, &overlapped, &bytesTransferred, false)) {
            lastError = ::GetLastError();
            std::cout << "IPC Write Get Overlapped Result error " << m_lastError << std::endl;
            //file_logger::SNL(LL_ERROR, [this](std::ostream& os) { os << "IPC Write Get Overlapped Result error " << m_lastError << std::endl; });
            success = false;
         }
      }
   }
   ioProxy->CloseHandle(overlapped.hEvent);
   return success;
#endif
}