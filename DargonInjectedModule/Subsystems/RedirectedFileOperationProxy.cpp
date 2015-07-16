#include "stdafx.h"
#include "RedirectedFileOperationProxy.hpp"

using namespace dargon::Subsystems;

RedirectedFileOperationProxy::RedirectedFileOperationProxy(std::shared_ptr<dargon::IO::IoProxy> io_proxy, std::string path)
   : DefaultFileOperationProxy(io_proxy), path(path) { }

HANDLE RedirectedFileOperationProxy::Create(LPCWSTR lpFilePath, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {
   return DefaultFileOperationProxy::Create(dargon::wide(path).c_str(), dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
}