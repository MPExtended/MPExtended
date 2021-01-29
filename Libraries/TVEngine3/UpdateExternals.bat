@echo off
cls
Title Updating externals

setlocal EnableDelayedExpansion

set LOG=update.log
echo. > %LOG%

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%
:CONT
IF NOT EXIST "%PROGS%\Team MediaPortal\MediaPortal\" SET PROGS=C:

echo PROGS=%PROGS% >> %LOG%

:: Predefined folders
set MP_PROG=%PROGS%\Team MediaPortal\MediaPortal
set MP_TVPR=%PROGS%\Team MediaPortal\MediaPortal TV Server
set MP_PLUG=%MP_PROG%\plugins\Windows
set MP_PROC=%MP_PROG%\plugins\process

FOR %%i IN (*.dll) DO (
	echo %%i
	if exist !MP_PROG!\%%i (
		echo Found %%i in !MP_PROG! >> %LOG%
		copy /y "!MP_PROG!\%%i" . >> %LOG%
	) else (
		if exist !MP_PLUG!\%%i (
			echo Found %%i in !MP_PLUG! >> %LOG%
			copy /y "!MP_PLUG!\%%i" . >> %LOG%
		) else (
			if exist !MP_PROC!\%%i (
				echo Found %%i in !MP_PROC! >> %LOG%
				copy /y "!MP_PROC!\%%i" . >> %LOG%
			) else (
    			if exist !MP_TVPR!\%%i (
    				echo Found %%i in !MP_PROC! >> %LOG%
    				copy /y "!MP_TVPR!\%%i" . >> %LOG%
            )
		)
	)
)
