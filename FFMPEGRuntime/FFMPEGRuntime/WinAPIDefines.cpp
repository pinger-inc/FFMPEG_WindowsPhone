#include "pch.h"

#include <windows.h>

/**
 * @brief Stub definitions of Windows API's used by FFMPEG but not available on WinRT.
 * @todo Stub implementations TBD
 */
extern "C"
{
    BOOL GetConsoleMode(HANDLE hConsoleHandle, LPDWORD lpMode)
    {
        return 0;
    }

    HANDLE GetStdHandle(DWORD nStdHandle)
    {
        return nullptr;
    }

    BOOL PeekNamedPipe(HANDLE hNamedPipe,
                       LPVOID lpBuffer,
                       DWORD nBufferSize,
                       LPDWORD lpBytesRead,
                       LPDWORD lpTotalBytesAvail,
                       LPDWORD lpBytesLeftThisMessage)
    {
        return 0;
    }

    HMODULE LoadLibraryA(const char* lpFileName)
    {
        return nullptr;
    }
}
