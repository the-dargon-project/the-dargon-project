#include "stdafx.h"
#include <Windows.h>
#include "DefaultFileOperationProxy.hpp"
#include "../IO/IoProxy.hpp"

using namespace dargon::IO;
using namespace dargon::Subsystems;

static_assert(sizeof(DWORD) == sizeof(uint32_t), "Size of DWORD must equal size of uint32_t.");
static_assert(sizeof(LONG) == sizeof(uint32_t), "Size of LONG must equal size of uint32_t.");

DefaultFileOperationProxy::DefaultFileOperationProxy(std::shared_ptr<IoProxy> io_proxy)
   : handle(NULL), io_proxy(io_proxy) { }

HANDLE DefaultFileOperationProxy::Create(LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {
   handle = io_proxy->CreateFileW(lpFilePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
   return handle;
}

BOOL DefaultFileOperationProxy::Read(void * buffer, uint32_t byte_count, OUT uint32_t * bytes_read, LPOVERLAPPED lpOverlapped) {
   return io_proxy->ReadFile(handle, buffer, byte_count, (unsigned long*)bytes_read, lpOverlapped);
}

BOOL DefaultFileOperationProxy::Write(const void * lpBuffer, uint32_t byte_count, OUT uint32_t * bytes_written, LPOVERLAPPED lpOverlapped) {
   return io_proxy->WriteFile(handle, lpBuffer, byte_count, (unsigned long*)bytes_written, lpOverlapped);
}

DWORD DefaultFileOperationProxy::Seek(int32_t distance_to_move, int32_t * distance_to_move_high, DWORD dwMoveMethod) {
   return io_proxy->SetFilePointer(handle, distance_to_move, (long*)distance_to_move_high, dwMoveMethod);
}

BOOL DefaultFileOperationProxy::Close() {
   return io_proxy->CloseHandle(handle);
}
