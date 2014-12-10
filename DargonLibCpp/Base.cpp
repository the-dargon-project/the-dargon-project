#include "dlc_pch.hpp"
#include "Base.hpp"
using namespace dargon;

Blob::Blob(UINT32 newSize) : size(newSize), data(new UINT8[newSize]) {}
Blob::Blob(UINT32 newSize, UINT8* newData) : size(newSize), data(newData) {} 
Blob::~Blob() { delete data; }