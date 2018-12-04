taskkill /IM dotnet.exe /F /T
dotnet-sonarscanner begin /k:"F1uctus_Axion" /d:sonar.organization="f1uctus-github" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="99f88f432ee524746d0b2dada15738fdc0f95d12"
dotnet build
rem nunit3-console "C:\Users\Fluctus\Documents\Code\CSharp\Axionlang\Axion.Testing\Axion.Testing.csproj"
dotnet-sonarscanner end /d:sonar.login="99f88f432ee524746d0b2dada15738fdc0f95d12"
pause