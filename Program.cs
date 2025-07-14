using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleAgenticAI.Models;
using SimpleAgenticAI.Services;

namespace SimpleAgenticAI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var azureSettings = configuration.GetSection("AzureOpenAI").Get<AzureOpenAISettings>();
            
            if (azureSettings == null || string.IsNullOrEmpty(azureSettings.ApiKey))
            {
                Console.WriteLine("❌ Azure OpenAI settings not configured properly.");
                Console.WriteLine("Please check your appsettings.json file.");
                Console.WriteLine("Make sure you have:");
                Console.WriteLine("- Endpoint URL");
                Console.WriteLine("- API Key");
                Console.WriteLine("- Deployment Name");
                return;
            }

            // Setup logging
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<AIAgentService>();

            try
            {
                // Create AI Agent
                var aiAgent = new AIAgentService(azureSettings, logger);

                Console.WriteLine("🤖 Simple Agentic AI Assistant Started!");
                Console.WriteLine("Type 'help' for available commands or 'exit' to quit.\n");

                while (true)
                {
                    Console.Write("You: ");
                    var input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    if (input.ToLower() == "exit")
                        break;

                    if (input.ToLower() == "help")
                    {
                        ShowHelp();
                        continue;
                    }

                    try
                    {
                        Console.WriteLine("🤖 AI: Processing your request...\n");
                        
                        string response;
                        
                        // Route different types of requests
                        if (input.StartsWith("/plan ", StringComparison.OrdinalIgnoreCase))
                        {
                            var task = input.Substring(6);
                            response = await aiAgent.AnalyzeAndPlanAsync(task);
                        }
                        else if (input.StartsWith("/decide ", StringComparison.OrdinalIgnoreCase))
                        {
                            var parts = input.Substring(8).Split(" | ");
                            if (parts.Length >= 2)
                            {
                                var scenario = parts[0];
                                var options = parts.Skip(1).ToArray();
                                response = await aiAgent.MakeDecisionAsync(scenario, options);
                            }
                            else
                            {
                                response = "Please use format: /decide scenario | option1 | option2 | option3";
                            }
                        }
                        else
                        {
                            response = await aiAgent.ProcessUserRequestAsync(input);
                        }

                        Console.WriteLine($"🤖 AI: {response}\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error: {ex.Message}\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to initialize AI Agent: {ex.Message}");
                Console.WriteLine("Please check your Azure OpenAI configuration.");
            }

            Console.WriteLine("Goodbye! 👋");
        }

        static void ShowHelp()
        {
            Console.WriteLine("\n📋 Available Commands:");
            Console.WriteLine("• Regular chat: Just type your message");
            Console.WriteLine("• /plan <task>: Create a step-by-step plan for a task");
            Console.WriteLine("• /decide <scenario> | <option1> | <option2> | <option3>: Get help making decisions");
            Console.WriteLine("• help: Show this help message");
            Console.WriteLine("• exit: Quit the application\n");
            
            Console.WriteLine("📝 Examples:");
            Console.WriteLine("• What is machine learning?");
            Console.WriteLine("• /plan organize a team meeting");
            Console.WriteLine("• /decide choose a programming language | Python | C# | JavaScript\n");
        }
    }
}