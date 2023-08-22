using Microsoft.SemanticKernel.SemanticFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SemanticChat
{
    internal class Settings
    {
        /// <summary>
        /// Gets a value-tuple of settings to configure the Azure OpenAI service
        /// </summary>
        public static (string key, string endpoint) GetEnvironmentSettings()
        {
            var key = Environment.GetEnvironmentVariable("AzureOpenAIKey");
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var endpoint = Environment.GetEnvironmentVariable("AzureOpenAIEndpoint");
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            return (key, endpoint);
        }

        public static PromptTemplateConfig GetDefaultConfig()
        {
            return new PromptTemplateConfig
            {
                Completion =
                {
                    MaxTokens = 2000,
                    Temperature = 0.7,
                    TopP = 0.5
                }
            };
        }
    }
}
