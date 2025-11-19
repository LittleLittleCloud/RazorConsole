// Copyright (c) RazorConsole. All rights reserved.

namespace LLMAgentTUI.Services;

public interface IChatService
{
    Task<string> SendMessageAsync(string message);
}
