#pragma once

#include "dargon.hpp"

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
#define DSP_EX_C2S_USER_OP_LOW                  ((BYTE)0x00)

#define DSP_EX_C2S_DIM_BOOTSTRAP_GET_ARGS       ((BYTE)0x01)
#define DSP_EX_C2S_DIM_READY_FOR_TASKS          ((BYTE)0x02)
#define DSP_EX_C2S_DIM_REMOTE_LOG               ((BYTE)0x03)

#define DSP_EX_C2S_USER_OP_HIGH                 ((BYTE)0x7F)

#define DSP_EX_C2S_SYSTEM_OP_LOW                ((BYTE)0x80)
#define DSP_EX_C2S_META_GET_DARGON_VERSION      ((BYTE)0x80)

#define DSP_EX_C2S_CONSOLE_OPEN                 ((BYTE)0x90)
#define DSP_EX_C2S_CONSOLE_WRITELINE            ((BYTE)0x91)
#define DSP_EX_C2S_CONSOLE_CLOSE                ((BYTE)0x9F)

#define DSP_EX_C2S_SYSTEM_OP_HIGH               ((BYTE)0xFF)

//-------------------------------------------------------------------------------------------------
// S2C or C2S Opcodes
//-------------------------------------------------------------------------------------------------

#define DSP_EX_ECHO                             ((BYTE)0xFE)
#define DSP_EX_EVENT_QUIT                       ((BYTE)0xFF)