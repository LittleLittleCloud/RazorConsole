using LLMAgentTUI.Components;
using LLMAgentTUI.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using RazorConsole.Core;

// Get API key from environment variable or use Ollama as default
var useOllama = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

await AppHost.RunAsync<App>(null, builder =>
{
    builder.ConfigureServices(services =>
    {
        if (useOllama)
        {
            // Use Ollama with local model
            services.AddChatClient(client =>
                new OllamaChatClient(new Uri("http://localhost:11434"), "llama3.2"));
        }
        else
        {
            // Use OpenAI
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
            services.AddChatClient(client =>
                new OpenAIClient(apiKey).AsChatClient("gpt-4o-mini"));
        }

        services.AddSingleton<IChatService, ChatService>();
    });

    builder.Configure(options =>
    {
        options.AutoClearConsole = false;
    });
});
