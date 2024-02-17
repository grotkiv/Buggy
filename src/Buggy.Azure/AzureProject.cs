namespace Buggy
{
    public class AzureProject
    {
        public string Url { get; set; } = "https://dev.azure.com";

        public string Organization { get; set; } = string.Empty;

        public string Project { get; set; } = string.Empty;

        public string Pat { get; set; } = string.Empty;

        public string Query { get; set; } = string.Empty;
    }
}