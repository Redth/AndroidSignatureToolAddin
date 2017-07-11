#!/bin/bash

nuget restore VS.Mac/AndroidKeystoreSignatureTool.VS.Mac.sln

xbuild /p:Configuration=Release VS.Mac/AndroidKeystoreSignatureTool.VS.Mac.sln

/Applications/Xamarin\ Studio.app/Contents/MacOS/mdtool setup pack VS.Mac/AndroidKeystoreSignatureTool.VS.Mac/bin/Release/AndroidKeystoreSignatureTool.VS.Mac.dll