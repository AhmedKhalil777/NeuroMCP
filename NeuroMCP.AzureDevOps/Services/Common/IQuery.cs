namespace NeuroMCP.AzureDevOps.Services.Common;

/// <summary>
/// Represents a query that retrieves data without changing state
/// </summary>
public interface IQuery<TResult>
{
    /// <summary>
    /// Executes the query and returns a result
    /// </summary>
    Task<TResult> ExecuteAsync();
}