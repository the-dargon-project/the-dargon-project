#include "stdafx.h"
#include <Windows.h>
#include "io/IoProxy.hpp"
#include "DefaultFileOperationProxy.hpp"

using namespace dargon::IO;
using namespace dargon::Subsystems;

static_assert(sizeof(DWORD) == sizeof(uint32_t), "Size of DWORD must equal size of uint32_t.");
static_assert(sizeof(LONG) == sizeof(uint32_t), "Size of LONG must equal size of uint32_t.");

DefaultFileOperationProxy::DefaultFileOperationProxy(std::shared_ptr<IoProxy> io_proxy)
   : FileOperationProxy(), handle(INVALID_HANDLE_VALUE), io_proxy(io_proxy), name("") { }

HANDLE DefaultFileOperationProxy::Create(LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {
   if (name.size() != 0) {
      std::cout << "Create called again!?" << std::endl;
      __debugbreak();
   }
   handle = io_proxy->CreateFileW(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
//   name.assign(dargon::narrow(lpFilePath));
   return handle;
}

BOOL DefaultFileOperationProxy::Read(void * buffer, uint32_t byte_count, OUT uint32_t * bytes_read, LPOVERLAPPED lpOverlapped) {
   return io_proxy->ReadFile(handle, buffer, byte_count, (unsigned long*)bytes_read, lpOverlapped);
}

BOOL DefaultFileOperationProxy::Write(const void * lpBuffer, uint32_t byte_count, OUT uint32_t * bytes_written, LPOVERLAPPED lpOverlapped) {
   return io_proxy->WriteFile(handle, lpBuffer, byte_count, (unsigned long*)bytes_written, lpOverlapped);
}

DWORD DefaultFileOperationProxy::Seek(int64_t distance_to_move, int64_t* new_file_pointer, DWORD dwMoveMethod) {
   LARGE_INTEGER li_distance_to_move;
   li_distance_to_move.QuadPart = distance_to_move;
   return io_proxy->SetFilePointerEx(handle, li_distance_to_move, (PLARGE_INTEGER)new_file_pointer, dwMoveMethod);
}

BOOL DefaultFileOperationProxy::Close() {
   return io_proxy->CloseHandle(handle);
}

std::string DefaultFileOperationProxy::ToString() { return name; }