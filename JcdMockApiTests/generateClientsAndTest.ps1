# IMPORTANT NOTE: This script expects JcdMockApi to be running at http://localhost:5000. 

npm i autorest

# Call Autorest turn a Swagger document into generated client classes we can use to talk to our API.
# Consumes the Swagger file created by calling ..\JcdMockApi\buildAndGenerateSwagger.ps1 
# Gets Autorest config from readme.md
./node_modules/.bin/autorest --input-file=".\swaggerV2.json"

# Rebuild to use the newly generated client classes.
# --no-incremental option marks the build as unsafe for incremental build and forces a clean rebuild of the project's dependency graph.
dotnet build --no-incremental

# Switch to our unit test directory and run tests.
#Set-Location ../JcdMockApiTests

# Rebuild the tests, which have a project reference to JcdMockApi to get the client classes. 
# Outputting the client classes to the tests directory would eliminate this step. 
#dotnet build

# Run the tests - you'll need to have an API running for them to pass. 
dotnet test