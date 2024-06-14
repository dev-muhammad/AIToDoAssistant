using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Configuration
{
    public class OpenAISettings
    {
        [Required]
        public string ApiKey { get; set; }

        [Required]
        public string Endpoint { get; set; }

        [Required]
        public string DeploymentName { get; set; }
    }
}
