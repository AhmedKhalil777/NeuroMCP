using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Models
{
    /// <summary>
    /// Interface that defines the basic functionality of an MCP tool
    /// </summary>
    public interface IMcpTool
    {
        /// <summary>
        /// Gets the name of the tool
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the version of the tool
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Gets a description of the tool
        /// </summary>
        string Description { get; }
    }
}