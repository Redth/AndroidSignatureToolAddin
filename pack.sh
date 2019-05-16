#!/bin/bash

msbuild /t:Restore VS.Mac/AndroidKeystoreSignatureTool.VS.Mac.sln

msbuild /t:Rebuild /p:Configuration=Release VS.Mac/AndroidKeystoreSignatureTool.VS.Mac.sln

/Applications/Visual\ Studio.app/Contents/MacOS/vstool setup pack VS.Mac/AndroidKeystoreSignatureTool.VS.Mac/bin/Release/AndroidKeystoreSignatureTool.VS.Mac.dll