#pragma once

#include "stdafx.h"
#include <boost/signals2.hpp>
#include <functional>
#include <detours.h>

#define  DIM_DECL_LIST(DataType, ListContentName) \
         static list<DataType>* m_my##ListContentName##List; \
         public: \
         static bool Add##ListContentName(DataType callback); \
         static bool Remove##ListContentName(DataType callback);

#define  DIM_IMPL_LIST(ClassName, DataType, ListContentName) \
         list<DataType>* ClassName::m_my##ListContentName##List = new list<DataType>(); \
         bool ClassName::Add##ListContentName(DataType callback) \
         { \
            m_my##ListContentName##List->push_back(callback); \
            return true; \
         } \
         bool ClassName::Remove##ListContentName(DataType callback) \
         { \
            m_my##ListContentName##List->remove(callback); \
            return true; \
         }

#define  DIM_DECL_STATIC_EVENT(CallbackFunctionType, EventName) \
         private: \
         static boost::signals2::signal<CallbackFunctionType> On##EventName; \
         public: \
         static boost::signals2::connection Add##EventName##Handler(boost::signals2::signal<CallbackFunctionType>::slot_type callback);

#define  DIM_IMPL_STATIC_EVENT(ClassName, CallbackFunctionType, EventName) \
         boost::signals2::signal<CallbackFunctionType> ClassName::On##EventName; \
         boost::signals2::connection ClassName::Add##EventName##Handler(boost::signals2::signal<CallbackFunctionType>::slot_type callback) \
         { \
            return ClassName::On##EventName.connect(callback); \
         }

#define DIM_INVOKE_EVENT(eventList, DelegateTypeName, ...) eventList(##__VA_ARGS__);
//#define DIM_INVOKE_CANCELLABLE_EVENT(eventList, DelegateTypeName, ...) \
//        list<DelegateTypeName>::iterator it##eventList; \
//        BOOL wasCancelled##eventList = false; \
//        for(it##eventList = eventList->begin(); it##eventList != eventList->end(); it##eventList++) \
//        if((*it##eventList)(##__VA_ARGS__)) { wasCancelled##eventList = true; break; }

#define  DIM_DECL_DETOUR(DetouredMethodName, DelegateTypeName) \
         static DelegateTypeName* m_real##DetouredMethodName;  \
         static DelegateTypeName* m_tramp##DetouredMethodName; \
         DIM_DECL_STATIC_EVENT(DelegateTypeName##NoCC, Pre##DetouredMethodName); /**/ \
         DIM_DECL_STATIC_EVENT(DelegateTypeName##NoCC, Post##DetouredMethodName); /**/ \
         public: \
         static void Install##DetouredMethodName##Detour(DWORD* pVtable); \
         static void Uninstall##DetouredMethodName##Detour(); \
         private: 

#define  DIM_IMPL_DETOUR(ClassName, DetouredMethodName, DelegateTypeName, vTableOffset, myDetouredMethodImplementation) \
         DelegateTypeName* ClassName::m_real##DetouredMethodName = NULL; \
         DelegateTypeName* ClassName::m_tramp##DetouredMethodName = NULL; \
         DIM_IMPL_STATIC_EVENT(ClassName, DelegateTypeName##NoCC, Pre##DetouredMethodName); /**/ \
         DIM_IMPL_STATIC_EVENT(ClassName, DelegateTypeName##NoCC, Post##DetouredMethodName); /**/ \
         void ClassName::Install##DetouredMethodName##Detour(DWORD* pVtable) \
         { \
            if(m_tramp##DetouredMethodName == NULL) /**/ \
            { /**/ \
               /* Logger::GetOutputStream(LL_INFO) << "Attempt Detour: " #DetouredMethodName " " << endl; /**/ \
               m_real##DetouredMethodName = (DelegateTypeName*)(pVtable[vTableOffset]); /**/ \
               /* Logger::GetOutputStream(LL_INFO) << "real" #DetouredMethodName ": " << m_real##DetouredMethodName << endl; /**/ \
               m_tramp##DetouredMethodName = (DelegateTypeName*)DetourFunction((PBYTE)m_real##DetouredMethodName, (PBYTE)&myDetouredMethodImplementation); /**/ \
               /* Logger::GetOutputStream(LL_INFO) << "Done!" << endl; /**/ \
            }else{ /**/ \
               /* Logger::GetOutputStream(LL_INFO) << "Already detoured " #DetouredMethodName "!: " << m_real##DetouredMethodName << endl; /**/ \
            } /**/\
         } \
         void ClassName::Uninstall##DetouredMethodName##Detour() \
         { \
            if(m_tramp##DetouredMethodName != NULL) \
            { \
               DetourRemove((BYTE*)m_tramp##DetouredMethodName, (BYTE*)m_real##DetouredMethodName); \
            } \
         }

#define  DIM_DECL_STATIC_DETOUR(ClassName, FriendlyFunctionName, DetouredMethodType, DetouredMethodName, TrampolineName) \
         static DetouredMethodType* m_real##FriendlyFunctionName;  \
         static DetouredMethodType* m_tramp##FriendlyFunctionName; \
         DIM_DECL_STATIC_EVENT(DetouredMethodType##NoCC, Pre##FriendlyFunctionName); /**/ \
         DIM_DECL_STATIC_EVENT(DetouredMethodType##NoCC, Post##FriendlyFunctionName); /**/ \
         public: \
         static void Install##FriendlyFunctionName##Detour(HMODULE hModule); \
         static void Uninstall##FriendlyFunctionName##Detour(); \
         private: 

#define  DIM_IMPL_STATIC_DETOUR(ClassName, FriendlyFunctionName, DetouredMethodType, DetouredMethodName, TrampolineName) \
         DetouredMethodType* ClassName::m_real##FriendlyFunctionName = NULL; \
         DetouredMethodType* ClassName::m_tramp##FriendlyFunctionName = NULL; \
         DIM_IMPL_STATIC_EVENT(ClassName, DetouredMethodType##NoCC, Pre##FriendlyFunctionName); /**/ \
         DIM_IMPL_STATIC_EVENT(ClassName, DetouredMethodType##NoCC, Post##FriendlyFunctionName); /**/ \
         void ClassName::Install##FriendlyFunctionName##Detour(HMODULE ModuleHandle) \
         { \
            if(m_tramp##FriendlyFunctionName == NULL) /**/ \
            { /**/ \
               std::cout << "Attempt Detour: " #FriendlyFunctionName " " << std::endl; /**/ \
               m_real##FriendlyFunctionName = (DetouredMethodType*)GetProcAddress(ModuleHandle, DetouredMethodName); /**/ \
               std::cout << "real" #FriendlyFunctionName ": " << m_real##FriendlyFunctionName << std::endl; /**/ \
               m_tramp##FriendlyFunctionName = (DetouredMethodType*)DetourFunction((PBYTE)m_real##FriendlyFunctionName, (PBYTE)&TrampolineName); /**/ \
               std::cout << "Done!" << std::endl; /**/ \
            }else{ /**/ \
               std::cout << "Already detoured " #FriendlyFunctionName "!: " << m_real##FriendlyFunctionName << std::endl; /**/ \
            } /**/ \
         } \
         void ClassName::Uninstall##FriendlyFunctionName##Detour() \
         { \
            if(m_tramp##FriendlyFunctionName != NULL) \
            { \
               DetourRemove((BYTE*)m_tramp##FriendlyFunctionName, (BYTE*)m_real##FriendlyFunctionName); \
            } \
         }