#include <iostream>
#include <Windows.h>
#include <cstdint>
#include <vector>
#include <random>
#include <algorithm>

#undef min;

struct chunk_t {
   std::int32_t lowerInclusive;
   std::int32_t upperExclusive;
};

std::vector<chunk_t*> generate_chunks(std::int32_t length);

int main() {
   std::cout << "Ghetto RADS Dummy - Press any key to continue..." << std::endl;
   getchar();

   auto hFile = CreateFileW(L"C:/dummy-files/a.txt", GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
   std::cout << "Created hFile: " << std::hex << hFile << std::endl;

   auto fileLength = static_cast<std::int32_t>(SetFilePointer(hFile, 0, nullptr, FILE_END));
   std::cout << "Observable file length: " << fileLength << std::endl;

   std::cout << "Generating chunks at which we'll read the file: " << std::endl;
   auto chunks = std::move(generate_chunks(fileLength));
   std::mt19937 random(0);

   goto rand_rel_hop;

   std::cout << "Dumping file sequentially: " << std::endl;
   auto hClone = CreateFileW(L"C:/dummy-files/clone_seq_nohop.txt", GENERIC_WRITE, FILE_SHARE_READ, nullptr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
   SetFilePointer(hFile, 0, nullptr, FILE_BEGIN);
   SetFilePointer(hClone, 0, nullptr, FILE_BEGIN);
   for (auto chunk : chunks) {
      auto chunkSize = chunk->upperExclusive - chunk->lowerInclusive;

      std::int8_t* buffer = new std::int8_t[chunkSize];
      DWORD bytesRemaining = chunkSize;
      DWORD totalBytesRead = 0;
      while (bytesRemaining > 0) {
         DWORD bytesRead;
         ReadFile(hFile, buffer + totalBytesRead, bytesRemaining, &bytesRead, NULL);
         bytesRemaining -= bytesRead;
         totalBytesRead += bytesRead;
      }

      bytesRemaining = chunkSize;
      DWORD totalBytesWritten = 0;
      while (bytesRemaining > 0) {
         DWORD bytesWritten;
         WriteFile(hClone, buffer + totalBytesWritten, bytesRemaining, &bytesWritten, NULL);
         bytesRemaining -= bytesWritten;
         totalBytesWritten += bytesWritten;
      }
   }
   CloseHandle(hClone);

   std::cout << "Dumping file sequentially (with hops): " << std::endl;
   hClone = CreateFileW(L"C:/dummy-files/clone_seq_hop.txt", GENERIC_WRITE, FILE_SHARE_READ, nullptr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
   for (auto chunk : chunks) {
      SetFilePointer(hFile, chunk->lowerInclusive, nullptr, FILE_BEGIN);
      SetFilePointer(hClone, chunk->lowerInclusive, nullptr, FILE_BEGIN);

      auto chunkSize = chunk->upperExclusive - chunk->lowerInclusive;

      std::int8_t* buffer = new std::int8_t[chunkSize];
      DWORD bytesRemaining = chunkSize;
      DWORD totalBytesRead = 0;
      while (bytesRemaining > 0) {
         DWORD bytesRead;
         ReadFile(hFile, buffer + totalBytesRead, bytesRemaining, &bytesRead, NULL);
         bytesRemaining -= bytesRead;
         totalBytesRead += bytesRead;
      }

      bytesRemaining = chunkSize;
      DWORD totalBytesWritten = 0;
      while (bytesRemaining > 0) {
         DWORD bytesWritten;
         WriteFile(hClone, buffer + totalBytesWritten, bytesRemaining, &bytesWritten, NULL);
         bytesRemaining -= bytesWritten;
         totalBytesWritten += bytesWritten;
      }
   }
   CloseHandle(hClone);

   std::cout << "Dumping file sequentially (with random hops): " << std::endl;
   std::shuffle(chunks.begin(), chunks.end(), random);
   hClone = CreateFileW(L"C:/dummy-files/clone_rand_hop.txt", GENERIC_WRITE, FILE_SHARE_READ, nullptr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
   for (auto chunk : chunks) {
      SetFilePointer(hFile, chunk->lowerInclusive, nullptr, FILE_BEGIN);
      SetFilePointer(hClone, chunk->lowerInclusive, nullptr, FILE_BEGIN);

      auto chunkSize = chunk->upperExclusive - chunk->lowerInclusive;

      std::int8_t* buffer = new std::int8_t[chunkSize];
      DWORD bytesRemaining = chunkSize;
      DWORD totalBytesRead = 0;
      while (bytesRemaining > 0) {
         DWORD bytesRead;
         ReadFile(hFile, buffer + totalBytesRead, bytesRemaining, &bytesRead, NULL);
         bytesRemaining -= bytesRead;
         totalBytesRead += bytesRead;
      }

      bytesRemaining = chunkSize;
      DWORD totalBytesWritten = 0;
      while (bytesRemaining > 0) {
         DWORD bytesWritten;
         WriteFile(hClone, buffer + totalBytesWritten, bytesRemaining, &bytesWritten, NULL);
         bytesRemaining -= bytesWritten;
         totalBytesWritten += bytesWritten;
      }
   }
   CloseHandle(hClone);

   rand_rel_hop:
   std::cout << "Dumping file sequentially (with random relative hops): " << std::endl;
   hClone = CreateFileW(L"C:/dummy-files/clone_rand_hop.txt", GENERIC_WRITE, FILE_SHARE_READ, nullptr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
   std::shuffle(chunks.begin(), chunks.end(), random);
   std::int32_t lastOffset = 0;
   SetFilePointer(hFile, 0, nullptr, FILE_BEGIN);
   SetFilePointer(hClone, 0, nullptr, FILE_BEGIN);

   for (auto chunk : chunks) {
      auto seek_offset = chunk->lowerInclusive - lastOffset;
      SetFilePointer(hFile, seek_offset, nullptr, FILE_CURRENT);
      SetFilePointer(hClone, seek_offset, nullptr, FILE_CURRENT);

      auto chunkSize = chunk->upperExclusive - chunk->lowerInclusive;

      std::cout << "Supposedly seeked " << seek_offset << " and copying from " << std::dec << chunk->lowerInclusive << " count " << chunkSize << std::endl;

      std::int8_t* buffer = new std::int8_t[chunkSize];
      DWORD bytesRemaining = chunkSize;
      DWORD totalBytesRead = 0;
      while (bytesRemaining > 0) {
         DWORD bytesRead;
         ReadFile(hFile, buffer + totalBytesRead, bytesRemaining, &bytesRead, NULL);
         bytesRemaining -= bytesRead;
         totalBytesRead += bytesRead;
      }

      bytesRemaining = chunkSize;
      DWORD totalBytesWritten = 0;
      while (bytesRemaining > 0) {
         DWORD bytesWritten;
         WriteFile(hClone, buffer + totalBytesWritten, bytesRemaining, &bytesWritten, NULL);
         bytesRemaining -= bytesWritten;
         totalBytesWritten += bytesWritten;
      }

      lastOffset = chunk->upperExclusive;
   }
   CloseHandle(hClone);
   CloseHandle(hFile);

   return 0;
}

std::vector<chunk_t*> generate_chunks(std::int32_t length) {
   std::default_random_engine re(0);
   std::uniform_int_distribution<std::int32_t> chunk_size_distribution(1, 32768);

   std::vector<chunk_t*> chunks;
   for (std::int32_t start = 0; start < length; ) {
      auto bytesRemaining = length - start;
      auto chunkSize = chunk_size_distribution(re);
      chunkSize = std::min(bytesRemaining, chunkSize);

      auto chunk = new chunk_t();
      chunk->lowerInclusive = start;
      chunk->upperExclusive = start + chunkSize;
      chunks.push_back(chunk);

      start += chunkSize;
   }
   return chunks;
}