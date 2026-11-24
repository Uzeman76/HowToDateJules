namespace ConsoleApp2;

public class Choice
{
    public int Number { get; set; }
    public string Text { get; set; } = string.Empty;
    public string TargetSituationId { get; set; } = string.Empty;
    public int AffectionChange { get; set; } = 0; // Ex: +15, -10, 0
}

public class ChoiceRecord
{
    public string SceneTitle { get; set; } = string.Empty;
    public string ChoiceText { get; set; } = string.Empty;
    public int AffectionChange { get; set; }

    public bool EstBonChoix => AffectionChange > 0;
    public bool EstMauvaisChoix => AffectionChange < 0;
    public bool EstNeutre => AffectionChange == 0;
}

public class Situation
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<Choice> Choices { get; set; } = new();
    public string? EndingMessage { get; set; }
    public string? NextScenarioFile { get; set; }

    public bool IsEnding => !string.IsNullOrWhiteSpace(EndingMessage) || Choices.Count == 0;
}

public class ScenarioData
{
    public string DateTitle { get; set; } = "Rendez-vous avec Jules";
    public string? DefaultNextScenario { get; set; }
    public Dictionary<string, Situation> Situations { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
