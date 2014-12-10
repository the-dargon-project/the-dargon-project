#pragma once 

#include "../../dargon.hpp"

#define DSP_OK                        ((BYTE)0x00)
#define DSP_DONE                      (DSP_OK)
#define DSP_CONTINUE                  ((BYTE)0x01)

#define DSP_GSM_DONE                  ((BYTE)0x00)
#define DSP_GSM_SEND                  ((BYTE)0x01)
#define DSP_GSM_SEND_EXPLICIT         ((BYTE)0x02)
#define DSP_GSM_SEND_BUFFER           ((BYTE)0x03)
#define DSP_GSM_SEND_BUFFER_EXPLICIT  ((BYTE)0x04)
#define DSP_GSM_SEND_FILE             ((BYTE)0x05)

#define DSP_WRITELINE                 ((BYTE)0x10)

#define DSP_GET_DARGON_VERSION        ((BYTE)0x20)
#define DSP_GET_STATIC_MODIFICATIONS  ((BYTE)0x40)

#define DSP_SET_INTERACTIVE           ((BYTE)0xE0)

#define DSP_EX_INIT                   ((BYTE)0xFE)

#define DSP_GOODBYE                   ((BYTE)0xFF)