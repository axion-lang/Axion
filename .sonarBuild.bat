taskkill /IM dotnet.exe /F /T

"C:\Users\Fluctus\.dotnet\tools\dotnet-sonarscanner.exe" begin /k:"F1uctus_Axion" /d:sonar.organization="f1uctus-github" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="99f88f432ee524746d0b2dada15738fdc0f95d12"

dotnet build

"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe" Axion.sln /t:Rebuild /p:VisualStudioVersion=15.0;Configuration=Debug;Platform="Any CPU"

rem nunit3-console "C:\Users\Fluctus\Documents\Code\CSharp\Axionlang\Axion.Testing\Axion.Testing.csproj"

"C:\Users\Fluctus\.dotnet\tools\dotnet-sonarscanner.exe" end /d:sonar.login="99f88f432ee524746d0b2dada15738fdc0f95d12"

pause