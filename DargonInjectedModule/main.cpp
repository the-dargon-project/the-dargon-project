#include "stdafx.h"
#include <Windows.h>
#include "Core.hpp"
using namespace std;
using namespace Dargon::InjectedModule;

int main(int argc, wchar_t* argv[])
{
   new Core(GetModuleHandle(NULL));
   //Core::Main(GetModuleHandle(NULL)); //GMH(NULL) returns the current process's handle
   return 0;
}