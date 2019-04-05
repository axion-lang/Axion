cd "C:\Users\F1uctus\Documents\Code\CSharp\Axion\"

taskkill /IM dotnet.exe /F /T

dotnet sonarscanner begin /k:"F1uctus_Axion" /d:sonar.organization="f1uctus-github" /d:sonar.host.url="https://sonarcloud.io"

"C:\Program Files (x86)\Microsoft Visual Studio\2019\Preview\MSBuild\Current\Bin\MSBuild.exe" Axion.sln -m /t:Rebuild /p:VisualStudioVersion=15.0;Configuration=Debug;Platform="Any CPU"
rem dotnet test "./Axion.Testing/Axion.Testing.csproj"

dotnet sonarscanner end

pause