using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace ToDoAssistant.Services
{
    public class OpenAIService
    {
        private readonly OpenAIClient _client;
        private readonly string _deploymentName;
        private readonly FunctionCallService _functionCallService;

        public OpenAIService(IConfiguration configuration, FunctionCallService functionCallService)
        {
            var apiKey = configuration["OpenAI:ApiKey"];
            var endpoint = configuration["OpenAI:Endpoint"];
            _deploymentName = configuration["OpenAI:DeploymentName"];
            _client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            _functionCallService = functionCallService;
        }

        public async Task<string> AskAsync(string userQuestion, List<ChatCompletionsFunctionToolDefinition> tools)
        {
            var messages = new List<ChatRequestMessage>
            {
                new ChatRequestSystemMessage("You are a AI assitant for ToDo list. You can ask me to add, remove, mark as done or show all ToDo items. For adding item use add_todo, for marking as done use mark_done, for removing item use remove_todo and for showing all items use show_all_todos. Never use unknown functions. While showing all items show as ordered list with format: Id. Description (done or pending) in the list. For any other questions, reply that you can not help."),
                new ChatRequestUserMessage(userQuestion)
            };

            var completionOptions = new ChatCompletionsOptions
            {
                Temperature = 0.5f,
                MaxTokens = 800,
                NucleusSamplingFactor = 0.95f,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
                DeploymentName = _deploymentName
            };
            completionOptions.ToolChoice = ChatCompletionsToolChoice.Auto;
            foreach (var tool in tools)
                completionOptions.Tools.Add(tool);

            foreach (var message in messages)
                completionOptions.Messages.Add(message);

            var response = await _client.GetChatCompletionsAsync(completionOptions);

            // == If the response includes a tool call, handle it and continue the conversation ==========
            ChatChoice responseChoice = response.Value.Choices[0];
            if (responseChoice.FinishReason == CompletionsFinishReason.ToolCalls)
            {
                // == Include the FunctionCall message in the conversation history ==========
                completionOptions.Messages.Add(new ChatRequestAssistantMessage(responseChoice.Message));

                // == Add a new tool message for each tool call that is resolved ==========
                foreach (ChatCompletionsToolCall toolCall in responseChoice.Message.ToolCalls)
                {
                    var ToolCallMsg = await _functionCallService.ExecuteFunctionAsync(toolCall);
                    completionOptions.Messages.Add(ToolCallMsg);
                }

                response = await _client.GetChatCompletionsAsync(completionOptions);
            }

            return response.Value.Choices[0].Message.Content;
        }
    }

}
