#pragma once

#include "../dlc_pch.hpp"
#include <Windows.h>
#include "../util.hpp"

//-------------------------------------------------------------------------------------------------
// ::CreateEventA
//-------------------------------------------------------------------------------------------------
typedef HANDLE(WINAPI FunctionCreateEventA)(LPSECURITY_ATTRIBUTES lpEventAttributes, BOOL bManualReset, BOOL bInitialState, LPCSTR lpName);
typedef FunctionCreateEventA* PFunctionCreateEventA;
typedef void (FunctionCreateEventANoCC)(LPSECURITY_ATTRIBUTES lpEventAttributes, BOOL bManualReset, BOOL bInitialState, LPCSTR lpName);
typedef FunctionCreateEventANoCC* PFunctionCreateEventANoCC;

//-------------------------------------------------------------------------------------------------
// ::CreateEventW
//-------------------------------------------------------------------------------------------------
typedef HANDLE(WINAPI FunctionCreateEventW)(LPSECURITY_ATTRIBUTES lpEventAttributes, BOOL bManualReset, BOOL bInitialState, LPCWSTR lpName);
typedef FunctionCreateEventW* PFunctionCreateEventW;
typedef void (FunctionCreateEventWNoCC)(LPSECURITY_ATTRIBUTES lpEventAttributes, BOOL bManualReset, BOOL bInitialState, LPCWSTR lpName);
typedef FunctionCreateEventWNoCC* PFunctionCreateEventWNoCC;

//-------------------------------------------------------------------------------------------------
// ::CreateFileA
//-------------------------------------------------------------------------------------------------
typedef HANDLE(WINAPI FunctionCreateFileA)(LPCSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
typedef FunctionCreateFileA* PFunctionCreateFileA;
typedef void (FunctionCreateFileANoCC)(LPCSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
typedef FunctionCreateFileANoCC* PFunctionCreateFileANoCC;

//-------------------------------------------------------------------------------------------------
// ::CreateFileW
//-------------------------------------------------------------------------------------------------
typedef HANDLE(WINAPI FunctionCreateFileW)(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
typedef FunctionCreateFileW* PFunctionCreateFileW;
typedef void (FunctionCreateFileWNoCC)(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
typedef FunctionCreateFileWNoCC* PFunctionCreateFileWNoCC;

//-------------------------------------------------------------------------------------------------
// ::ReadFile
//-------------------------------------------------------------------------------------------------
typedef BOOL(WINAPI FunctionReadFile)(HANDLE hFile, LPVOID lpBuffer, DWORD nNumberOfBytesToRead, LPDWORD lpNumberOfBytesRead, LPOVERLAPPED lpOverlapped);
typedef FunctionReadFile* PFunctionReadFile;
typedef void (FunctionReadFileNoCC)(HANDLE hFile, LPVOID lpBuffer, DWORD nNumberOfBytesToRead, LPDWORD lpNumberOfBytesRead, LPOVERLAPPED lpOverlapped);
typedef FunctionReadFileNoCC* PFunctionReadFileNoCC;

//-------------------------------------------------------------------------------------------------
// ::WriteFile
//-------------------------------------------------------------------------------------------------
typedef BOOL(WINAPI FunctionWriteFile)(HANDLE hFile, LPCVOID lpBuffer, DWORD nNumberOfBytesToWrite, LPDWORD lpNumberOfBytesWritten, LPOVERLAPPED lpOverlapped);
typedef FunctionWriteFile* PFunctionWriteFile;
typedef void (FunctionWriteFileNoCC)(HANDLE hFile, LPCVOID lpBuffer, DWORD nNumberOfBytesToWrite, LPDWORD lpNumberOfBytesWritten, LPOVERLAPPED lpOverlapped);
typedef FunctionWriteFileNoCC* PFunctionWriteFileNoCC;

//-------------------------------------------------------------------------------------------------
// ::CloseHandle
//-------------------------------------------------------------------------------------------------
typedef BOOL(WINAPI FunctionCloseHandle)(HANDLE hObject);
typedef FunctionCloseHandle* PFunctionCloseHandle;
typedef void (FunctionCloseHandleNoCC)(HANDLE hObject);
typedef FunctionCloseHandleNoCC* PFunctionCloseHandleNoCC;

//-------------------------------------------------------------------------------------------------
// ::SetFilePointer
//-------------------------------------------------------------------------------------------------
typedef DWORD(WINAPI FunctionSetFilePointer)(HANDLE hFile, LONG lDistanceToMove, PLONG lpDistanceToMoveHigh, DWORD dwMoveMethod);
typedef FunctionSetFilePointer* PFunctionSetFilePointer;
typedef void (FunctionSetFilePointerNoCC)(HANDLE hFile, LONG lDistanceToMove, PLONG lpDistanceToMoveHigh, DWORD dwMoveMethod);
typedef FunctionSetFilePointerNoCC* PFunctionSetFilePointerNoCC;

namespace dargon { namespace IO { 
   class IoProxy
   {
      PFunctionCreateEventA _createEventA;
      PFunctionCreateEventW _createEventW;
      PFunctionCreateFileA _createFileA;
      PFunctionCreateFileW _createFileW;
      PFunctionReadFile _readFile;
      PFunctionWriteFile _writeFile;
      PFunctionCloseHandle _closeHandle;
      PFunctionSetFilePointer _setFilePointer;

   public:
      IoProxy() : _createEventA(nullptr), _createEventW(nullptr), _createFileA(nullptr), _createFileW(nullptr), _readFile(nullptr), _writeFile(nullptr), _closeHandle(nullptr), _setFilePointer(nullptr) {}

      void Initialize() {
         auto hKernel32 = ::WaitForModuleHandle("Kernel32.dll");
         _createEventA = (PFunctionCreateEventA)::GetProcAddress(hKernel32, "CreateEventA");
         _createEventW = (PFunctionCreateEventW)::GetProcAddress(hKernel32, "CreateEventW");
         _createFileA = (PFunctionCreateFileA)::GetProcAddress(hKernel32, "CreateFileA");
         _createFileW = (PFunctionCreateFileW)::GetProcAddress(hKernel32, "CreateFileW");
         _readFile = (PFunctionReadFile)::GetProcAddress(hKernel32, "ReadFile");
         _writeFile = (PFunctionWriteFile)::GetProcAddress(hKernel32, "WriteFile");
         _closeHandle = (PFunctionCloseHandle)::GetProcAddress(hKernel32, "CloseHandle");
         _setFilePointer = (PFunctionSetFilePointer)::GetProcAddress(hKernel32, "SetFilePointer");
      }

      inline HANDLE WINAPI CreateEventA(LPSECURITY_ATTRIBUTES lpEventAttributes, BOOL bManualReset, BOOL bInitialState, LPCSTR lpName) {
         return _createEventA(lpEventAttributes, bManualReset, bInitialState, lpName);
      }

      inline HANDLE WINAPI CreateEventW(LPSECURITY_ATTRIBUTES lpEventAttributes, BOOL bManualReset, BOOL bInitialState, LPCWSTR lpName) {
         return _createEventW(lpEventAttributes, bManualReset, bInitialState, lpName);
      }

      inline HANDLE WINAPI CreateFileA(LPCSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {
         return _createFileA(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
      }

      inline HANDLE WINAPI CreateFileW(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {
         return _createFileW(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
      }

      inline BOOL WINAPI ReadFile(HANDLE hFile, LPVOID lpBuffer, DWORD nNumberOfBytesToRead, LPDWORD lpNumberOfBytesRead, LPOVERLAPPED lpOverlapped) {
         return _readFile(hFile, lpBuffer, nNumberOfBytesToRead, lpNumberOfBytesRead, lpOverlapped);
      }

      inline BOOL WINAPI WriteFile(HANDLE hFile, LPCVOID lpBuffer, DWORD nNumberOfBytesToWrite, LPDWORD lpNumberOfBytesWritten, LPOVERLAPPED lpOverlapped) {
         return _writeFile(hFile, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped);
      }

      inline BOOL WINAPI CloseHandle(HANDLE hObject) {
         return _closeHandle(hObject);
      }
      
      inline DWORD WINAPI SetFilePointer(HANDLE hFile, LONG lDistanceToMove, PLONG lpDistanceToMoveHigh, DWORD dwMoveMethod) {
         return _setFilePointer(hFile, lDistanceToMove, lpDistanceToMoveHigh, dwMoveMethod);
      }

      inline void __Override(PFunctionCreateEventA createEventA, PFunctionCreateEventW createEventW,
                             PFunctionCreateFileA createFileA, PFunctionCreateFileW createFileW,
                             PFunctionReadFile readFile, PFunctionWriteFile writeFile,
                             PFunctionCloseHandle closeHandle,
                             PFunctionSetFilePointer setFilePointer) {
         _createEventA = createEventA;
         _createEventW = createEventW;
         _createFileA = createFileA;
         _createFileW = createFileW;
         _readFile = readFile;
         _writeFile = writeFile;
         _closeHandle = closeHandle;
         _setFilePointer = setFilePointer;
      }
   };
} }