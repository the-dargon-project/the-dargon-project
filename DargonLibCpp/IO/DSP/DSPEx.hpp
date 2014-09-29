#pragma once

#include "../../Dargon.hpp"

typedef BYTE DSPEx;

#define DSP_EX_OK                               ((BYTE)0x00)
#define DSP_EX_DONE                             (DSP_EX_OK)
#define DSP_EX_CONTINUE                         ((BYTE)0x01)

//-------------------------------------------------------------------------------------------------
// S2C Event Opcodes
//-------------------------------------------------------------------------------------------------
#define DSP_EX_S2C_DIM_RUN_TASKS                ((BYTE)0xA1)

#define DSP_EX_S2C_EVENT_QUIT                   ((BYTE)0xE0)
#define DSP_EX_S2C_EVENT_RESOURCES_RELOAD       ((BYTE)0xE1)

//-------------------------------------------------------------------------------------------------
// C2S Event Opcodes
//-------------------------------------------------------------------------------------------------
#define DSP_EX_C2S_META_GET_DARGON_VERSION      ((BYTE)0x10)

#define DSP_EX_C2S_IO_GET_ROOT_INFO             ((BYTE)0x20)
#define DSP_EX_C2S_IO_GET_NODE_ROOT_INFO        ((BYTE)0x21)
#define DSP_EX_C2S_IO_GET_BREADCRUMBS_INFO      ((BYTE)0x22)
#define DSP_EX_C2S_IO_GET_NODE_CHILDREN         ((BYTE)0x23)
#define DSP_EX_C2S_IO_GET_NODE_INFO             ((BYTE)0x25)
#define DSP_EX_C2S_IO_GET_NODE_INFOS            ((BYTE)0x26)

#define DSP_EX_C2S_IO_RESOLVE                   ((BYTE)0x28)
#define DSP_EX_C2S_IO_BULK_RESOLVE              ((BYTE)0x29)
#define DSP_EX_C2S_IO_FREE_HANDLE               ((BYTE)0x30)

#define DSP_EX_C2S_MOD_LS_ROOT                  ((BYTE)0x50)

#define DSP_EX_C2S_CONSOLE_OPEN                 ((BYTE)0x70)
#define DSP_EX_C2S_CONSOLE_WRITELINE            ((BYTE)0x71)
#define DSP_EX_C2S_CONSOLE_CLOSE                ((BYTE)0x79)

#define DSP_EX_C2S_REMOTE_LOG                   ((BYTE)0x80)

#define DSP_EX_C2S_GAME_OP_LOW                  ((BYTE)0xA0)
#define DSP_EX_C2S_DIM_OP_LOW                   ((BYTE)0xA0)
#define DSP_EX_C2S_DIM_BOOTSTRAP_GET_ARGS       ((BYTE)0xA0)
#define DSP_EX_C2S_DIM_READY_FOR_TASKS          ((BYTE)0xA1)

#define DSP_EX_C2S_DIM_OP_HIGH                  ((BYTE)0xB9)
#define DSP_EX_C2S_GAME_OP_HIGH                 ((BYTE)0xEF)

//-------------------------------------------------------------------------------------------------
// S2C or C2S Opcodes
//-------------------------------------------------------------------------------------------------

#define DSP_EX_ECHO                             ((BYTE)0xFE)
#define DSP_EX_EVENT_QUIT                       ((BYTE)0xFF)