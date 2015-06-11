#include "stdafx.h"
#include <Windows.h>
#include "Init/bootstrap_context.hpp"
#include "Subsystem.hpp"
#include "Subsystems/FileSubsystem.hpp"
#include "Configuration.hpp"
#include "IO/DIM/CommandManager.hpp"
#include "vfm/vfm_reader.hpp"

// void remap_file(std::string dest, std::string vfmPath, std::shared_ptr<dargon::Subsystems::RemappedFileOperationProxyFactoryFactory> factory) {
// }

#define REMAP_FILE(DEST, VFM_PATH) 

int main(int argc, wchar_t* argv[]) {
   dargon::file_logger::initialize("C:/dim_command_line.log");
   auto logger = dargon::file_logger::instance();

   auto bootstrap_context = std::make_shared<dargon::Init::bootstrap_context>();
   bootstrap_context->module_handle = GetModuleHandle(NULL);
   bootstrap_context->logger = logger;
   bootstrap_context->dtp_node = nullptr;
   bootstrap_context->dtp_session = nullptr;
   bootstrap_context->io_proxy = std::make_shared<dargon::IO::IoProxy>();
   bootstrap_context->argument_flags.push_back(dargon::Configuration::EnableFileSystemHooksFlag);

   // initialize libvfm dependencies
   auto sector_factory = std::make_shared<dargon::vfm_sector_factory>(bootstrap_context->io_proxy);
   auto vfm_reader = std::make_shared<dargon::vfm_reader>(sector_factory);
   
   // load configuration
   auto configuration = dargon::Configuration::Parse(bootstrap_context->argument_flags, bootstrap_context->argument_properties);

   // construct command manager
   auto command_manager = std::make_shared<dargon::IO::DIM::CommandManager>(bootstrap_context->dtp_session, configuration);

   // initialize subsystem dependencies
   dargon::Subsystem::Initialize(bootstrap_context, configuration, logger);
   auto file_subsystem = std::make_shared<dargon::Subsystems::FileSubsystem>();
   file_subsystem->Initialize();

   // initialize command manager
   command_manager->Initialize();

   auto hFile = CreateFileW(L"C:/Riot Games/League of Legends/Config/input.ini", GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, NULL);
   LARGE_INTEGER distance_to_move;
   LARGE_INTEGER file_position;
   ZeroMemory(&distance_to_move, sizeof(distance_to_move));
   ZeroMemory(&file_position, sizeof(file_position));
   SetFilePointerEx(hFile, distance_to_move, &file_position, FILE_END);
   std::cout << file_position.QuadPart << std::endl;

//    auto remappedProxyFactoryFactory = std::make_shared<dargon::Subsystems::RemappedFileOperationProxyFactoryFactory>(bootstrap_context->io_proxy, vfm_reader);
//    auto hFile = CreateFileA(DEST_PATH, GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
//    BY_HANDLE_FILE_INFORMATION fileInformation;
//    GetFileInformationByHandle(hFile, &fileInformation);
//    dargon::Subsystems::FileIdentifier file_identifier;
//    file_identifier.targetFileIndexHigh = fileInformation.nFileIndexHigh;
//    file_identifier.targetFileIndexLow = fileInformation.nFileIndexLow;
//    file_identifier.targetVolumeSerialNumber = fileInformation.dwVolumeSerialNumber;
//    file_subsystem->AddFileOverride(file_identifier, remappedProxyFactoryFactory->create(VFM_PATH));

//   file_subsystem->AddFileOverride(new FileOperationP)
//    Application::HandleDllEntry(GetModuleHandle(NULL));
//   while (true);
   return 0;
}