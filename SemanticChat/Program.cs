using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SemanticFunctions;
using System;
using System.Threading.Tasks;
using Spectre.Console;

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

            const string skPrompt = @"
You are Kevin, a helpful bot that writes using concise business language, adopted from the 
HBR Guide to Better Business Writing. You will answer questions, provide instruction
or answer 'I do not know' if you do not have an answer. You will be very concise and avoid 
overly verbose language.

{{$history}}
User: {{$userInput}}
Kevin:";

            var promptTemplate = new PromptTemplate(skPrompt, Settings.GetDefaultConfig(), kernel);
            var functionConfig = new SemanticFunctionConfig(Settings.GetDefaultConfig(), promptTemplate);
            var chatFunction = kernel.RegisterSemanticFunction("ChatBot", "Chat", functionConfig);

            var context = kernel.CreateNewContext();

            // Initialize a chat history for this context
            var history = "";
            context.Variables["history"] = history;

            while (true) // Infinite loop until "exit" command
            {
                // Display chat history in a yellow box
                if (!string.IsNullOrEmpty(history))
                {
                    AnsiConsole.Render(
                        new Panel("[yellow]" + history.Trim() + "[/]")
                        .Header("[yellow]Context - Chat History[/]")
                        .BorderColor(Color.Yellow)
                    );
                    AnsiConsole.WriteLine();
                }

                // Display prompt with cyan color for user input
                AnsiConsole.Markup("[cyan]User: [/]");
                var userInput = Console.ReadLine();

                // Exit the loop if the user types "exit"
                if (userInput?.ToLower().Trim() == "exit")
                    break;

                context.Variables["userInput"] = userInput;

                var bot_answer = await chatFunction.InvokeAsync(context);

                // Add a horizontal line if there's existing history
                if (!string.IsNullOrEmpty(history))
                {
                    history += "[yellow]──────[/]\n";
                }
                history += $"\nUser: {userInput}\nKevin: {bot_answer.Result}\n";
                context.Variables.Update(history);

                var fullResponse = bot_answer.Result;
                var kevinResponseStart = fullResponse.LastIndexOf("Kevin: ") + 7;  // +7 to account for the length of "Kevin: "
                var kevinResponse = fullResponse.Substring(kevinResponseStart).Trim();
                Console.WriteLine("");

                // Display Kevin's response with dark orange color
                AnsiConsole.MarkupLine("[darkorange]Kevin: {0}\n[/]", kevinResponse);
            }
        }
    }
}
