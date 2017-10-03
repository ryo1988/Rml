call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsDevCmd.bat" 
cd /d %~dp0
msbuild /t:pack /p:Configuration=Release /p:IncludeSymbols=true
pause