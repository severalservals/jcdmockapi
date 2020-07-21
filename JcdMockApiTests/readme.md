# JcdMockAPI and general Swashbuckle/Autorest install instructions

This file contains the configuration for generating the JcdMockApi from the Swagger/OpenAPI specification.

Below is the magic string that tells AutoRest to treat this file as a literate configuration file. 
> see https://aka.ms/autorest

``` yaml
output-folder: Generated
```

``` yaml
csharp: # just having a 'csharp' node enables the use of the csharp generator. 
# Seems like you do want at least one subnode or the csharp generator won't kick in. !?!?!?
  namespace: JcdMockApi
```

Below is from a page on how to get past the error "Error: OperationId is required for all operations" for Swagger 2.0. 
It doesn't solve the problem for OpenAPI 3.0. 

``` yaml
# list all the input OpenAPI files (may be YAML, JSON, or Literate- OpenAPI markdown)
# input-file: 
#  - http://transport.opendata.ch/swagger.json

# this allows you to programatically tweak the swagger file before it is modeled.
directive:
  from: swagger-document # do it globally 
  where: $.paths.*.* 
  
  # set each operationId to 'Transport_<Tag>'
  # It's pretty arbitrary. It can be anything but the name of the project itself. 
  transform: $.operationId = `Transport_${$.tags[0]}`
```