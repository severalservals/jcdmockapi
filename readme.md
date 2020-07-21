This project shows how you can use Swashbuckle to generate service-description JSON for an API, then use Autorest to generate client classes from that JSON and use the client classes to call the API for an API test. To see it run end to end:

# Go into JcdMockApi and execute the script to install the Swashbuckle CLI, build the API and generate the JSON
cd JcdMockApi
.\buildAndGenerateSwagger.ps1
# Run the API
dotnet run
# With the API still running, go into JcdMockApiTests and execute the script to generate the client classes from the JSON and then run a test using those client classes. 
cd ..\JcdMockApiTests
.\