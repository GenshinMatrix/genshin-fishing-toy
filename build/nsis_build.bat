cd /d %~dp0
rename GenshinFishingToySetup_*_x64.cer App.cer
rename GenshinFishingToySetup_*_x64.msixbundle App.msixbundle
del App_Setup.exe
nsis\tools\makensis .\nsis\setup.nsi
del GenshinFishingToySetup.exe
rename App_Setup.exe GenshinFishingToySetup.exe
@pause
