#include "stdafx.h"
#include <Windows.h>
#include "Application.hpp"
using namespace std;
using namespace dargon;

int main(int argc, wchar_t* argv[])
{
   Application::HandleDllEntry(GetModuleHandle(NULL));
   return 0;
}