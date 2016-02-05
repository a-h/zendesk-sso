.\.nuget\nuget.exe restore -PackagesDirectory .\packages
"C:\program Files (x86)\msBuild\14.0\Bin\msbuild.exe" .\ZendeskSSO\ZendeskSSO.csproj /t:Build
copy .\ZendeskSSO\bin\*.dll .\ZendeskSSOWebsite\bin\
