namespace MDMPI.App.Core.Common.Services
{
    public class GeminiSettings
    {
        public const string SectionName = "GeminiAI";

        /// <summary>
        /// Your Gemini API key.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// The Gemini model to use (e.g., "gemini-2.5-flash").
        /// </summary>
        public string Model { get; set; } = "gemini-2.5-flash";
        
        /// <summary>
        /// The default prompt to send to Gemini for each request.
        /// </summary>
        public string Prompt { get; set; } = string.Empty;
    }
}
