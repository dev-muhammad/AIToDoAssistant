using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDoAssistant.Services;
using Azure;
using Azure.AI.OpenAI;

namespace ToDoAssistant.Hubs
{
    public class ChatHub : Hub
    {
        private readonly OpenAIService _openAIService;
        private readonly FunctionCallService _functionCallService;

        public ChatHub(OpenAIService openAIService, FunctionCallService functionCallService)
        {
            _openAIService = openAIService;
            _functionCallService = functionCallService;
        }

        public async Task SendMessage(string message)
        {
            var tools = new List<ChatCompletionsFunctionToolDefinition>
            {
                _functionCallService.GetAddToDoTool(),
                _functionCallService.GetMarkDoneTool(),
                _functionCallService.GetRemoveToDoTool(),
                _functionCallService.GetShowAllToDoTool()
            };

            var result = await _openAIService.AskAsync(message, tools);
            await Clients.All.SendAsync("ReceiveMessage", result);
        }
    }
}
