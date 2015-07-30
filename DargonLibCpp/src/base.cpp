#include "dlc_pch.hpp"
#include "base.hpp"
using namespace dargon;

blob::blob(UINT32 newSize) : size(newSize), data(new UINT8[newSize]) {}
blob::blob(UINT32 newSize, UINT8* newData) : size(newSize), data(newData) {} 
blob::~blob() { delete data; }