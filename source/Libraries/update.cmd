@rem rd /s \\fusebox\public\feed
md c:\scratch\nuget
erase c:\scratch\nuget\*.nupkg
for /R %%s in (*.nupkg) do copy %%s c:\scratch\nuget
@rem nuget init c:\scratch\nuget \\fusebox\public\feed
