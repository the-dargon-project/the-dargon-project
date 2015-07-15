#pragma once

#include "stdafx.h"
#include <memory>
#include <unordered_set>

#include "logger.hpp"
#include "noncopyable.hpp"

#include "Configuration.hpp"
#include "Init/bootstrap_context.hpp"
#include "IO/DIM/IDIMCommandHandler.hpp"

namespace dargon {
   // Base Interface for a subsystem (formally Hooker in Dargon r1).
   // Note: All Subsystems should be treated like singletons.  Their instance methods should pass
   //       control to static methods.  As only one instance exists and all fields are static, 
   //       there is no differentiation between instance and static members.
   // See ISubsystem.Detours.hpp for information related to implementing a subsystem.
   class Subsystem : dargon::noncopyable
   {
      // - Static ---------------------------------------------------------------------------------
   protected:
      // file_logger object which must be initialized at the start of the Initialize method before hooks
      // are set.  The m_file_logger field is to be accessible by the macros defined in Detoursutil.hpp
      static std::shared_ptr<dargon::logger> s_logger;

      // Pointer to the Bootstrap Context which is guaranteed to be valid for the lifetime of the
      // subsystem.  The pointed-to memory is owned by the Dargon Injected Module's core and this
      // field is initialized in the Initialize() method.
      static std::shared_ptr<const dargon::Init::bootstrap_context> s_bootstrap_context;

      static std::shared_ptr<Configuration> Subsystem::s_configuration;

      // Gets the pointer to the given object's virtual method table.
      // pObject: Object whose virtual method table pointer we are getting
      // returns: Pointer to the object's virtual method table (an array of 32-bit addresses)
      static DWORD* GetVTablePointer(void* pObject);

      // Waits until GetModuleHandle(moduleName) returns a valid module handle.
      static HMODULE WaitForModuleHandle(const char* moduleName);

   public:
      // When the Dargon Core bootstraps, the static subsystem class stores the bootstrap context
      // and file_logger, which are required for use by subsystem instances.  The static subsystem
      // class essentially serves as a globally accessible way for subsystem implementations to 
      // access the bootstrap context.
      static void Initialize(std::shared_ptr<const dargon::Init::bootstrap_context> bootstrap_context, std::shared_ptr<Configuration> configuration, std::shared_ptr<dargon::logger> logger);

      // - Instance -------------------------------------------------------------------------------
   protected:
      bool m_initialized;

   protected:
      // Registers the subsystem in the list of subsystems
      Subsystem();
      ~Subsystem();

   public:
      // Initializes the Dargon Injected Module subsystem.  
      // Superclasses must invoke this method on initialization for DIM Commands to work
      // returns: true if the object is now initialized
      virtual bool Initialize();

      // Uninitializes the Dargon Injected Module subsystem.
      // For the Dargon Alpha build, this is not required to be implemented.
      // Superclasses must invoke this method on initialization for DIM Commands to work
      // returns: true if the object is now uninitialized
      virtual bool Uninitialize();
      
      // Superclasses must invoke this method on initialization for DIM Commands to work
      // returns: Whether or not the subsystem is currently initialized. 
      //          This should be FALSE on object construction.
      virtual bool IsInitialized();
   };
}