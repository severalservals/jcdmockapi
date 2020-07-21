# Build our app and for the Swashbuckle CLI which we've installed as a dotnet tool. 
dotnet tool restore

# Build project.
dotnet build



# Use the DLL that is the output of the build command above to generate a Swagger document describing our API.
# Last argument is whatever value is passed as the "name" argument to the SwaggerDoc() method call in Startup.cs.
dotnet swagger tofile --serializeasv2 --output "..\JcdMockApiTests\swaggerV2.json" ".\bin\Debug\netcoreapp2.2\JcdMockApi.dll" "JCDMockApi"

