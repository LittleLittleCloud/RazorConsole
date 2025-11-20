// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.Extensions.AI;

namespace LLMAgentTUI.Services;

public class ChatService : IChatService
{
    private readonly IChatClient _chatClient;
    private readonly List<ChatMessage> _conversationHistory = new();

    public ChatService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> SendMessageAsync(string message)
    {
        _conversationHistory.Add(new ChatMessage(ChatRole.User, message));

        var response = await _chatClient.CompleteAsync(_conversationHistory).ConfigureAwait(false);

        var assistantMessage = response.Message.Text ?? "No response from the AI.";
        _conversationHistory.Add(new ChatMessage(ChatRole.Assistant, assistantMessage));

        return assistantMessage;
    }
}
