#include "stdafx.h"
#include <windows.h>
#include <stdio.h>
#include <fcntl.h>
#include <io.h>
#include <iostream>
#include <cstdio>
#include <fstream>

#ifndef _USE_OLD_IOSTREAMS
using namespace std;
#endif

// maximum mumber of lines the output console should have
static const WORD MAX_CONSOLE_LINES = 500;

void RedirectIOToConsole()
{
//   __debugbreak();
   int hConHandle;
   long lStdHandle;

   CONSOLE_SCREEN_BUFFER_INFO coninfo;
   FILE *fp;

   auto oldStdIn = GetStdHandle(STD_INPUT_HANDLE);
   auto oldStdOut = GetStdHandle(STD_OUTPUT_HANDLE);

   // allocate a console for this app
   if (AllocConsole() == FALSE) {
      auto error = GetLastError();
      std::cout << "AllocConsole failed with " << error << " old stdin " << oldStdIn << " stdout " << oldStdOut << std::endl;
   } else {
      // set the screen buffer to be big enough to let us scroll text
      GetConsoleScreenBufferInfo(GetStdHandle(STD_OUTPUT_HANDLE), &coninfo);
      coninfo.dwSize.Y = MAX_CONSOLE_LINES;
      SetConsoleScreenBufferSize(GetStdHandle(STD_OUTPUT_HANDLE), coninfo.dwSize);

      // redirect unbuffered STDIN to the console
      lStdHandle = (long)GetStdHandle(STD_INPUT_HANDLE);
      hConHandle = _open_osfhandle(lStdHandle, _O_TEXT);
      fp = _fdopen(hConHandle, "r");
      *stdin = *fp;
      setvbuf(stdin, NULL, _IONBF, 0);

      // redirect unbuffered STDOUT to the console
      lStdHandle = (long)GetStdHandle(STD_OUTPUT_HANDLE);
      hConHandle = _open_osfhandle(lStdHandle, _O_TEXT);
      fp = _fdopen(hConHandle, "w");
      *stdout = *fp;
      setvbuf(stdout, NULL, _IONBF, 0);

      // redirect unbuffered STDERR to the console
      lStdHandle = (long)GetStdHandle(STD_ERROR_HANDLE);
      hConHandle = _open_osfhandle(lStdHandle, _O_TEXT);
      fp = _fdopen(hConHandle, "w");
      *stderr = *fp;
      setvbuf(stderr, NULL, _IONBF, 0);

      // make cout, wcout, cin, wcin, wcerr, cerr, wclog and clog
      // point to console as well
      //std::cout.sync_with_stdio();
      std::wcout.sync_with_stdio();
      std::cerr.sync_with_stdio();
      std::wcerr.sync_with_stdio();
      std::cin.sync_with_stdio();
      std::wcin.sync_with_stdio();
      std::clog.sync_with_stdio();
      std::wclog.sync_with_stdio();

      // Redirecting cout normally didn't work but this works so we'll pray to jesus
      // that it'll still work in production
      // Alias Cout to Cerr lol
      std::cout.rdbuf(std::cerr.rdbuf());
   }
}

//End of File