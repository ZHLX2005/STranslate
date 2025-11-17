namespace STranslate.Plugin.Ocr.OpenAI;

public class Settings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Url { get; set; } = "https://api.openai.com/";
    public bool AutoTransBack { get; set; } = false;
    public string Model { get; set; } = "gpt-4o";
    public List<string> Models { get; set; } =
    [
        "gpt-4o",
        "gpt-5",
    ];
    public int MaxTokens { get; set; } = 2048;
    public double Temperature { get; set; } = 0.7;
    public int TopP { get; set; } = 1;
    public int N { get; set; } = 1;
    public bool Stream { get; set; } = true;
    public int? MaxRetries { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 1000;

    public List<Prompt> Prompts { get; set; } =
    [
        new("文本识别",
        [
            // https://github.com/skitsanos/gemini-ocr/blob/main/ocr.sh
            new PromptItem("user", "Act like a text scanner. Extract text as it is without analyzing it and without summarizing it. Treat all images as a whole document and analyze them accordingly. Think of it as a document with multiple pages, each image being a page. Understand page-to-page flow logically and semantically.")
        ], true)
    ];
}