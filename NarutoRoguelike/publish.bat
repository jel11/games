@echo off
echo ============================================================
echo  Naruto Roguelike -- Windows x64 Self-Contained Publish
echo ============================================================
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./build
echo.
echo Done. Output: .\build\NarutoRoguelike.exe
pause
