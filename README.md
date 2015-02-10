Building FFMPEG for Windows Phone

Prerequisites
------------

1) Visual Studio 2012 with Update 2

2) MinGw + MSYS (http://www.mingw.org/).

3) Install c99-to-c89-1.0.3 and add the location to the PATH environment variable.

4) Install vsyasm-1.3.0-win64 and add the location to the PATH environment variable.

5) Download gas-preprocessor.pl (https://github.com/FFmpeg/gas-preprocessor), and copy it into msys bin folder.

6) zlib library that is required for png support is already checked-in to the 3rd Party folder. The current version of the library is obtained from Nuget Package Manager: https://www.nuget.org/packages/zlib_wp8/

~~~
External dependencies ( c99-to-c89-1.0.3  +  vsyasm-1.3.0-win64 + gas-preprocessor.pl)  used for building the currently checked-in libraries are stored in ffmpeg-dependencies.zip 
~~~

Building
------------

1) cd into ffmpeg folder

2) Apply the patches from Git command line client by invoking:
```
./applyPatches.sh
```

3) Open "VS2012 ARM Cross Tools Command Prompt"

4) Start an msys window by invoking:
C:/MinGw/msys/1.0/bin/msys.bat

5) Using the newly opened msys+mingw window, change directory to ffmpeg.2.4:
```
cd ffmpeg/ffmpeg.2.4
```

6) Invoke the build script (no option means debug):
```
./build_ffmpeg_msvc.sh
```

If the build fails due to duplicate link.exe in msys, need to rename msys version of link.exe so that the visual studio link.exe is used.

7) In order to build in release mode:
```
./build_ffmpeg_msvc.sh release
```

8) For a quick build without re-configuring:
```
./build_ffmpeg_msvc.sh quick
```

9) Output static libraries will be stored in ffmpeg/build/<debug/release> folder

~~~
Currently checked-in ffmpeg.2.4.tar is derived from https://github.com/qyljcy/FFmpeg
~~~


Contributors
------------
[Derek Rodrigues] (https://github.com/DerekARodrigues)

[Santhoshkumar Sunderrajan] (https://github.com/santhosh-kumar)


Disclaimer
------------
Please comply to the original FFMPEG license (https://www.ffmpeg.org/legal.html) for the FFMPEG components.

Like FFmpeg itself, this library is LGPL 2.1+.
 
