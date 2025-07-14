using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SimpleAgenticAI.Models;

namespace SimpleAgenticAI.Services
{
    public class AIAgentService
    {
        private readonly Kernel _kernel;
        private readonly ILogger<AIAgentService> _logger;

        public AIAgentService(AzureOpenAISettings settings, ILogger<AIAgentService> logger)
        {
            _logger = logger;
            
            var builder = Kernel.CreateBuilder();
            builder.Services.AddLogging(c => c.AddConsole());
            
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: settings.DeploymentName,
                endpoint: settings.Endpoint,
                apiKey: settings.ApiKey
            );

            _kernel = builder.Build();
        }

        public async Task<string> ProcessUserRequestAsync(string userInput)
        {
            try
            {
                // Create a system prompt to make the AI act as an intelligent agent
                var systemPrompt = @"
                You are an intelligent AI assistant that can help with various tasks.
                Analyze the user's request and provide helpful, accurate responses.
                If the request involves multiple steps, break them down clearly.
                Be concise but thorough in your responses.
                ";

                var prompt = $"{systemPrompt}\n\nUser Request: {userInput}";
                
                var result = await _kernel.InvokePromptAsync(prompt);
                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing user request");
                return "I apologize, but I encountered an error while processing your request.";
            }
        }

        public async Task<string> AnalyzeAndPlanAsync(string task)
        {
            try
            {
                var prompt = $@"
                You are an AI planning assistant. Analyze the following task and create a step-by-step plan:
                
                Task: {task}
                
                Please provide:
                1. Task analysis
                2. Step-by-step plan
                3. Potential challenges
                4. Success criteria
                
                Be specific and actionable.
                ";

                var result = await _kernel.InvokePromptAsync(prompt);
                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing and planning task");
                return "I couldn't analyze the task. Please try again.";
            }
        }

        public async Task<string> MakeDecisionAsync(string scenario, string[] options)
        {
            try
            {
                var optionsText = string.Join("\n", options.Select((opt, i) => $"{i + 1}. {opt}"));
                
                var prompt = $@"
                You are an AI decision-making assistant. Analyze the following scenario and options:
                
                Scenario: {scenario}
                
                Options:
                {optionsText}
                
                Please provide:
                1. Analysis of each option
                2. Recommended decision with reasoning
                3. Potential outcomes
                
                Be logical and consider multiple factors.
                ";

                var result = await _kernel.InvokePromptAsync(prompt);
                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making decision");
                return "I couldn't analyze the decision. Please try again.";
            }
        }
    }
}