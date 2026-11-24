namespace ConsoleApp2;

public class Choice
{
    public int Number { get; set; }
    public string Text { get; set; } = string.Empty;
    public string TargetSituationId { get; set; } = string.Empty;
}

public class Situation
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<Choice> Choices { get; set; } = new();
    public string? EndingMessage { get; set; }

    public bool IsEnding => !string.IsNullOrWhiteSpace(EndingMessage) || Choices.Count == 0;
}
