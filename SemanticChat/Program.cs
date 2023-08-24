using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SemanticFunctions;
using System;
using System.Threading.Tasks;
using Spectre.Console;
using Microsoft.SemanticKernel.Orchestration;
using System.Diagnostics;

namespace SemanticChat
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var (key, endpoint) = Settings.GetEnvironmentSettings();

            var builder = new KernelBuilder();
            builder.WithAzureChatCompletionService("gpt-35-turbo", endpoint, key);

            IKernel kernel = builder.Build();

            string skPrompt = File.ReadAllText("Prompt.txt");

            var promptTemplate = new PromptTemplate(skPrompt, Settings.GetDefaultConfig(), kernel);
            var functionConfig = new SemanticFunctionConfig(Settings.GetDefaultConfig(), promptTemplate);
            var chatFunction = kernel.RegisterSemanticFunction("ChatBot", "Chat", functionConfig);

            var context = kernel.CreateNewContext();
            Console.Write("Enter city name: ");
            var city = Console.ReadLine();
            var outputFile = @"C:\temp\sample.html";
            context.Variables["userInput"] = city;

            var bot_answer = await chatFunction.InvokeAsync(context);
            var answer = bot_answer.ToString().Replace("```html", "").Replace("```", "");

            if (answer.Contains("<h2"))
            {
                var template = File.ReadAllText("Template.html")
                .Replace("{{BODY}}", answer.ToString())
                .Replace("{{LOCATION}}", city);
                File.WriteAllText(outputFile, template);


                Console.WriteLine(bot_answer);

                Process.Start(new ProcessStartInfo("cmd", $"/c start {outputFile}") { CreateNoWindow = true });
            }
            else
            {
                Console.WriteLine("No data.");
            }
        }
    }
}