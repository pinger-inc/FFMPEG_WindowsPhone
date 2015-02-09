#include "pch.h"
#include "Exceptions.h"

#include <sstream>

namespace FFMPEGRuntime
{
    /**
     * @brief Create error description string to use as exception text
     * @param resultCode
     * @param functionName
     * @param errorDescription
     * @returns error description
     */
    std::string FFMPEGError::GetErrorDescription( int resultCode, const std::string& functionName, const std::string& errorDescription )
    {
        std::stringstream description;
        description
	        << "Function '" << functionName
	        << "' "
	        << ( ( resultCode != 0 ) ? "failed" : "completed" )
	        << " with result code "
	        << resultCode
	        << " (" << errorDescription << ")";

        return description.str( );
    }
}
