if "%1" == "" goto end
pushd c:\scratch\nuget
for %%s in (*.nupkg) do dotnet nuget push %%s -s https://api.nuget.org/v3/index.json -k %1
goto end

:help
@echo publish nugetapikey
:end
popd
