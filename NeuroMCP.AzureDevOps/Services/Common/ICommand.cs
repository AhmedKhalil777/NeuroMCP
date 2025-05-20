namespace NeuroMCP.AzureDevOps.Services.Common;

/// <summary>
/// Represents a command that changes state
/// </summary>
public interface ICommand<TResult>
{
    /// <summary>
    /// Executes the command and returns a result
    /// </summary>
    Task<TResult> ExecuteAsync();
}