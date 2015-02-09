#include "pch.h"
#include "FFMPEGRuntime.h"
#include "Exceptions.h"

#ifdef _M_ARM
extern "C"
{
extern int C_ffmpeg_main(int argc, char** argv);
extern void C_ffmpeg_reset();
}
#else
int C_ffmpeg_main(int argc, char** argv)
{
    return 0;
}

extern void C_ffmpeg_reset()
{
}
#endif

#include <windows.h>

#include <string>
#include <vector>
#include <exception>
#include <locale>
#include <codecvt>
#include <memory>
#include <io.h>
#include <fstream>
#include <cassert>

using namespace FFMPEGRuntime;
using namespace Platform;

/**
 * @brief Log file name used for the ffmpeg report.
 */
static std::string g_logFilePath;

extern "C"
{
    /**
     * @brief Implementation of a symbol extern'd by the ffmpeg code to obtain
     * the file name that should be used for the ffmpeg report.
     * @note the string buffer returned by this method must remain in scope for as long as ffmpeg might use it
     * @returns log file name
     */
    const char* GetReportFileName( )
    {
        return g_logFilePath.c_str( ); 
    }
}


/**
 * @brief C'tor
 * @param logFilePath
 */
FFMpeg::FFMpeg( String^ logFilePath )
{
    g_logFilePath = Convert( logFilePath->Data( ) );
}


/**
 * @brief D'tor
 */
FFMpeg::~FFMpeg( )
{
}


/**
 * @brief Run ffmpeg with the given argument list.
 *        See http://www.ffmpeg.org/ffmpeg.html for usage.
 * @param commandArguments
 * @returns 0 on success; !=0 on failure
 */
unsigned int FFMpeg::Run( const Array<String^>^ commandArguments )
{
    int returnCode = 1;

#ifdef _M_ARM
    try
    {
        // Compile argument list from the given arguments
        std::vector<std::string> argumentList;

        // Prepend the 'process' name to the argument list
        // This is always expected to be the first argument
        argumentList.push_back( "ffmpeg" );

        // Only enable logging for debug builds
#if _DEBUG
        // Set log level to "debug" (48)
        argumentList.push_back( "-loglevel" );
        argumentList.push_back( "48" );

        // Dumps full command line and console output to file
        argumentList.push_back( "-report" );
#endif

        // Disable interaction on standard input (required)
        argumentList.push_back( "-nostdin" );

        // Copy the arguments from the input Array<String>
        for ( unsigned int i = 0; i < commandArguments->Length; ++i )
        {
            // Convert wide-char String (UCS-2) to std::string (multibyte UTF-8)
            argumentList.push_back( Convert( commandArguments[i]->Data( ) ) );
        }

        // Create the main() argument list - a char* array
        std::unique_ptr<char*[]> pCommand( new char*[argumentList.size( )] );

        for ( unsigned  int i = 0; i < argumentList.size( ); ++i )
        {
            pCommand[i] = const_cast<char*>( argumentList[i].c_str( ) );
        }

        // Pass the argument list to ffmpeg main()
        returnCode = ::C_ffmpeg_main( argumentList.size( ), pCommand.get( ) );
    }
    catch ( FFMPEGError& ex )
    {
        returnCode = ex.GetResultCode( );

        if ( returnCode != 0 )
        {
            // ffmpeg failed!

            // Log the exception
            LogLine( ex.what( ) );

#if _DEBUG
            // Dump the report log to the console
            PrintReport( );
#endif
        }
    }

    // Reset ffmpeg
    try
    {
        ::C_ffmpeg_reset();
    }
    catch ( std::exception& /*ex*/ )
    {
        // Ignored
    }

#endif

    // return the status
    // 0 - succeeded
    return returnCode;
}


/**
 * @brief Convert the given wide character string to a multi-byte string.
 * @param source
 * @returns multi-byte string
 */
std::string FFMpeg::Convert( const std::wstring& source )
{
    typedef std::codecvt_utf8<wchar_t> convert_type;
    std::wstring_convert<convert_type, wchar_t> converter;
    return converter.to_bytes( source );
}


/**
 * @brief Convert the given multi-byte string to a wide character string.
 * @param source
 * @returns wide char string
 */
std::wstring FFMpeg::Convert( const std::string& source )
{
    std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
    return converter.from_bytes(source);
}


/**
 * @brief Log the given message to the console
 * @param message
 */
void FFMpeg::LogLine( const std::string& message )
{
    OutputDebugString( Convert( message + "\n" ).c_str( ) );
}


/**
 * @brief Print the ffmpeg report file to the console.
 */
void FFMpeg::PrintReport( )
{
    try
    {
        std::ifstream inputFileStream;

        inputFileStream.exceptions( std::ifstream::badbit );
        inputFileStream.open( g_logFilePath.c_str( ) );

        std::string line;

        LogLine( "==========================================" );
        LogLine( "ffmpeg report file:");
        LogLine( "==========================================" );

        while ( ! inputFileStream.eof( )    &&
                std::getline( inputFileStream, line) )
        {
            if ( line.empty( ) )
            {
                continue;
            }

            LogLine( line );
        }

        LogLine( "==========================================" );

        inputFileStream.close( );
    }
    catch ( std::exception& )
    {
        // Ignore any exceptions thrown while attempting to log the report
    }
}
