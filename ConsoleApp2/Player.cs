namespace ConsoleApp2;

public class Player
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public DateTime SaveDate { get; set; } = DateTime.Now;

    public override string ToString()
    {
        return $"{Name} ({Age} ans, {Gender})";
    }
}
