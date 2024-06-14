using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace ToDoAssistant.Services
{
    public class FunctionCallService
    {
        private readonly ToDoService _toDoService;

        public FunctionCallService(ToDoService toDoService)
        {
            _toDoService = toDoService;
        }

        public async Task<ChatRequestToolMessage> ExecuteFunctionAsync(ChatCompletionsToolCall toolCall)
        {
            var functionToolCall = toolCall as ChatCompletionsFunctionToolCall;
            string arguments = functionToolCall.Arguments;
            var functionResultData = (object)null;
            switch (functionToolCall?.Name)
            {
                case "add_todo":
                    var description = JObject.Parse(arguments)["description"].ToString();
                    functionResultData = AddToDoItem(description);
                    return new ChatRequestToolMessage(functionResultData.ToString(), toolCall.Id);
                case "mark_done":
                    var idDone = int.Parse(JObject.Parse(arguments)["id"].ToString());
                    functionResultData = MarkToDoItemAsDone(idDone);
                    return new ChatRequestToolMessage(functionResultData.ToString(), toolCall.Id);
                case "remove_todo":
                    var idRemove = int.Parse(JObject.Parse(arguments)["id"].ToString());
                    functionResultData = RemoveToDoItem(idRemove);
                    return new ChatRequestToolMessage(functionResultData.ToString(), toolCall.Id);
                case "show_all_todos":
                    functionResultData = ShowAllToDoItems();
                    return new ChatRequestToolMessage(functionResultData.ToString(), toolCall.Id);
                default:
                    return new ChatRequestToolMessage("Unknown function", toolCall.Id);
            }
        }

        private string AddToDoItem(string description)
        {
            var item = _toDoService.Add(description);
            return $"Added ToDo: {item.Description}";
        }

        private string MarkToDoItemAsDone(int id)
        {
            _toDoService.MarkAsDone(id);
            return $"Marked ToDo {id} as done.";
        }

        private string RemoveToDoItem(int id)
        {
            _toDoService.Remove(id);
            return $"Removed ToDo {id}.";
        }

        private string ShowAllToDoItems()
        {
            var items = _toDoService.GetAll();
            return string.Join("\n", items.Select(item => $"ID: {item.Id}, Description: {item.Description}, Done: {item.IsDone}"));
        }

        public ChatCompletionsFunctionToolDefinition GetAddToDoTool()
        {
            return new ChatCompletionsFunctionToolDefinition
            {
                Name = "add_todo",
                Description = "Add a new ToDo item",
                Parameters = BinaryData.FromObjectAsJson(new
                {
                    type = "object",
                    properties = new
                    {
                        description = new
                        {
                            type = "string",
                            description = "The description of the ToDo item"
                        }
                    },
                    required = new[] { "description" }
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            };
        }

        public ChatCompletionsFunctionToolDefinition GetMarkDoneTool()
        {
            return new ChatCompletionsFunctionToolDefinition
            {
                Name = "mark_done",
                Description = "Mark a ToDo item as done",
                Parameters = BinaryData.FromObjectAsJson(new
                {
                    type = "object",
                    properties = new
                    {
                        id = new
                        {
                            type = "integer",
                            description = "The ID of the ToDo item"
                        }
                    },
                    required = new[] { "id" }
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            };
        }

        public ChatCompletionsFunctionToolDefinition GetRemoveToDoTool()
        {
            return new ChatCompletionsFunctionToolDefinition
            {
                Name = "remove_todo",
                Description = "Remove a ToDo item",
                Parameters = BinaryData.FromObjectAsJson(new
                {
                    type = "object",
                    properties = new
                    {
                        id = new
                        {
                            type = "integer",
                            description = "The ID of the ToDo item"
                        }
                    },
                    required = new[] { "id" }
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            };
        }

        public ChatCompletionsFunctionToolDefinition GetShowAllToDoTool()
        {
            return new ChatCompletionsFunctionToolDefinition
            {
                Name = "show_all_todos",
                Description = "Show all ToDo items",
                Parameters = BinaryData.FromObjectAsJson(new
                {
                    type = "object",
                    properties = new { }
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            };
        }
    }

}
