#include "pch.h"
#include "Exceptions.h"

#include <cassert>

extern "C"
{
    /**
     * @brief This function is a replacement for the use of exit() by ffmpeg source code.
     * ffmpeg uses exit() to return control in the case of a fatal failure or normal termination.
     * Since we don't want the process to exit, this function throws an exception instead. The exception will
     * be thrown through the ffmpeg source code and can be caught at the outermost layers.
     * @param resultCode - 0 on success; > 0 or < 0 otherwise 
     * @param pFunctionName - not NULL
     * @param pDescription - not NULL
     * @throws FFMPEGRuntime::FFMPEGError - always
     */
#pragma warning( disable : 4297 ) // function assumed not to throw an exception but does
    void ffmpeg_exit( int resultCode, const char* pFunctionName, const char* pDescription )
    {
	    assert( pFunctionName != NULL );
	    assert( pDescription != NULL );

	    throw FFMPEGRuntime::FFMPEGError( resultCode, pFunctionName, pDescription );
    }
}
