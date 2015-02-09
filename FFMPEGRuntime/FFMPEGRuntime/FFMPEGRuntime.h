#pragma once

#include <string>

namespace FFMPEGRuntime
{
    using namespace Windows::Foundation;
    using Platform::String;
    using Platform::Array;

    /**
     * @brief FFMpeg wrapper
     */
    public ref class FFMpeg sealed
    {
    public:
        FFMpeg( String^ logFilePath );
        virtual ~FFMpeg( );

        // Run ffmpeg using the given arbitrary argument list.
        // See ffmpeg documentation for the list of supported arguments.
        unsigned int Run( const Array<String^>^ commandArguments );

    private:
        static std::string Convert ( const std::wstring& source );
        static std::wstring Convert( const std::string& source );

        static void LogLine( const std::string& message );
        static void PrintReport( );
    };
}
