namespace ConsoleApp2;

public class Player
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public int Affection { get; set; } = 20; // Démarre à 20% (Intrigué)
    public string CurrentScenario { get; set; } = "situation_parc.txt";
    public DateTime SaveDate { get; set; } = DateTime.Now;

    public override string ToString()
    {
        return $"{Name} ({Age} ans, {Gender}, Amour: {Affection}%)";
    }
}
