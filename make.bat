@setlocal enabledelayedexpansion
@set PREVPROMPT=%PROMPT%
@prompt $E[1A
@set MAKE=make.bat
@set PROJ=PicoPDF
@echo on

@if "%1" == "" (set TARGET=build
) else (set TARGET=%1 && shift)

@call :%TARGET% %1 %2 %3 %4 %5 %6 %7 %8 %9
@prompt %PREVPROMPT%
@exit /b %ERRORLEVEL%

:build
	dotnet build --nologo -v q --clp:NoSummary
	@exit /b %ERRORLEVEL%

:clean
	dotnet clean --nologo -v q
	@exit /b %ERRORLEVEL%

:distclean
	@call :clean
	@for /F %%i in ('powershell -c Select-Xml -Path %PROJ%.slnx -XPath "//Solution/Project | ForEach-Object {$_.Node.Path}"') do @(
		echo rmdir /S /Q %%~dpibin
		rmdir /S /Q %%~dpibin 2>nul
		echo rmdir /S /Q %%~dpiobj
		rmdir /S /Q %%~dpiobj 2>nul
	)
	@exit /b %ERRORLEVEL%

:release
	@call :setenv VERSION_FILE "powershell -Command Get-Date -Format yyyyMMdd"
	git archive HEAD --output=%PROJ%-%VERSION_FILE%.zip
	
	dotnet publish src --nologo -v q --clp:NoSummary -c Release -o .tmp
	powershell -NoProfile $ProgressPreference = 'SilentlyContinue' ; Compress-Archive -Force -Path .tmp\*, README.md, LICENSE -DestinationPath %PROJ%-lib-%VERSION_FILE%.zip
	rmdir /S /Q .tmp 2>nul
	
	@exit /b %ERRORLEVEL%

:test
	dotnet test --nologo -v q %*
	@exit /b %ERRORLEVEL%

:test-all
	dotnet run --project test-all/%PROJ%.TestAll.csproj --no-launch-profile -- %*
	@exit /b %ERRORLEVEL%

:sample
	dotnet run --project test-all/%PROJ%.TestAll.csproj --no-launch-profile -- %* create --work-directory docs/sample --register-user-font docs/sample --debug false --contents-deflate true --cmap-deflate true
	dotnet run --project test-all/%PROJ%.TestAll.csproj --no-launch-profile -- %* manual-args --font docs/sample/NotoSansJP-Regular.ttf    -o docs/sample/subset-ttf.pdf TTF-subset
	dotnet run --project test-all/%PROJ%.TestAll.csproj --no-launch-profile -- %* manual-args --font docs/sample/NotoSansCJK-Regular.ttc,0 -o docs/sample/subset-cff.pdf CFF-subset
	@exit /b %ERRORLEVEL%

:bench
	dotnet run --project bench/%PROJ%.Benchmark.csproj --no-launch-profile -c Release %*
	@exit /b %ERRORLEVEL%

:publish
	@call :release
	
	@call :setenv VERSION_FILE "powershell -Command Get-Date -Format yyyyMMdd"
	@call :setenv VERSION_NAME "powershell -Command Get-Date -Format yyyy.M.d"
	@call :setenv BUILD_NAME   "powershell -Command Get-Date -Format HHmm"
	@set      VERSION=%VERSION_NAME%
	git tag %VERSION%
	git push origin %VERSION%
	gh release create %VERSION% %PROJ%-lib-%VERSION_FILE%.zip -t %VERSION% > nul
	@exit /b %ERRORLEVEL%

:setenv
	@for /f "usebackq delims=" %%x in (`%~2`) do @set %1=%%x
	@exit /b %ERRORLEVEL%
