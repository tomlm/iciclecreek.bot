rd /s \\fusebox\public\feed
erase c:\scratch\nuget\*.nupkg
for /R %%s in (*.nupkg) do copy %%s c:\scratch\nuget
nuget init c:\scratch\nuget \\fusebox\public\feed
