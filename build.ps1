dotnet test .\tests\openmedstack.linq2fhir.R4.tests\openmedstack.linq2fhir.R4.tests.csproj -c Release
dotnet test .\tests\openmedstack.linq2fhir.R4B.tests\openmedstack.linq2fhir.R4B.tests.csproj -c Release
dotnet test .\tests\openmedstack.linq2fhir.R5.tests\openmedstack.linq2fhir.R5.tests.csproj -c Release

dotnet pack .\src\openmedstack.linq2fhir.R4\openmedstack.linq2fhir.R4.csproj -o . -c Release
dotnet pack .\src\openmedstack.linq2fhir.R4B\openmedstack.linq2fhir.R4B.csproj -o . -c Release
dotnet pack .\src\openmedstack.linq2fhir.R5\openmedstack.linq2fhir.R5.csproj -o . -c Release
