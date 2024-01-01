@echo off
rd /s /q bin
echo Deleted current files...
mkdir bin
echo Created bin folder...
echo Calling sharpmake bootstrap...
cd sharpmake
dotnet build Sharpmake.sln -c Release
echo Sharpmake build finished
cd..
echo Copying files...
xcopy /s sharpmake\Sharpmake.Application\bin\Release\net6.0 bin

echo ********
echo **DONE**
echo ********
pause