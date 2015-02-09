#pragma once

#include <string>
#include <stdexcept>

namespace FFMPEGRuntime
{
    /**
     * @brief Exception thrown when the ffmpeg 3rd party source invokes ffmpeg_exit() to exit the current context.
     */
    class FFMPEGError : public std::runtime_error
    {
    public:
        FFMPEGError( int resultCode, const std::string& functionName, const std::string& errorDescription )
            : std::runtime_error( GetErrorDescription( resultCode, functionName, errorDescription ) ),
              m_resultCode( resultCode )
        {
        }

        int GetResultCode( ) const { return m_resultCode; }

    private:
        static std::string GetErrorDescription( int resultCode, const std::string& functionName, const std::string& errorDescription );

        const int m_resultCode;
    };
}
