using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Models
{
    /// <summary>
    /// Represents an account model for authentication purposes
    /// </summary>
    public class AccountModel
    {
        /// <summary>
        /// Gets or sets the ID of the account
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the display name of the account
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the email of the account
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the email address of the account (alias for Email)
        /// </summary>
        public string? EmailAddress
        {
            get => Email;
            set => Email = value;
        }

        /// <summary>
        /// Gets or sets the URL of the account
        /// </summary>
        public string? Url { get; set; }
    }
}