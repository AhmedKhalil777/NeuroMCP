# CQRS Architecture with MediatR

This directory contains the implementation of the Command Query Responsibility Segregation (CQRS) pattern using MediatR for the NeuroMCP.AzureDevOps project.

## Structure

The CQRS implementation is organized as follows:

- **Commands**: Contains command classes representing operations that change state
- **Queries**: Contains query classes representing operations that retrieve data
- **Models**: Contains model classes used by commands and queries
  - **Commands**: Model classes used for command operations
  - **Queries**: Model classes used for query operations
- **Common**: Common base classes and utilities

## Base Classes

- `AzureDevOpsRequest<TResponse>`: Base class for all requests (commands and queries)
- `AzureDevOpsRequestHandler<TRequest, TResponse>`: Base class for all request handlers

## Implementation Pattern

Each feature follows this pattern:

1. **Models**: Define data models in the `Models` directory
2. **Query/Command**: Create a request class in the appropriate directory
3. **Result**: Define the result model for the request
4. **Handler**: Implement the handler with the business logic
5. **Controller**: Expose the functionality via a controller endpoint

## Example

For the SearchCode feature:

1. `SearchCodeModel.cs`: Data model in Models/Queries
2. `SearchCodeQuery.cs`: Query class in Queries/SearchCode
3. `SearchCodeResult.cs`: Result model in Queries/SearchCode
4. `SearchCodeQueryHandler.cs`: Handler in Queries/SearchCode
5. `SearchController.cs`: Controller exposing the functionality

## Usage

```csharp
// In a controller
public async Task<ActionResult<SearchCodeResult>> SearchCodeAsync(
    [FromQuery] string searchText)
{
    var query = new SearchCodeQuery
    {
        SearchText = searchText
    };

    var result = await _mediator.Send(query);
    return Ok(result);
}
``` 