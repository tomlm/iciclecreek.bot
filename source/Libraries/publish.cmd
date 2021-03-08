@echo are you sure?
pause
for /R %%s in (*.nupkg) do dotnet nuget push %%s --api-key oy2bwtwiakv7coro3klq4ebn2c67ka6utj5k5oc62vu724 --source https://api.nuget.org/v3/index.json


