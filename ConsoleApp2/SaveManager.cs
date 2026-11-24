namespace ConsoleApp2;

public static class SaveManager
{
    public const string SaveFileName = "sauvegarde.txt";

    public static string GetSavePath()
    {
        if (File.Exists(SaveFileName))
        {
            return Path.GetFullPath(SaveFileName);
        }

        string appDirFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SaveFileName);
        if (File.Exists(appDirFile))
        {
            return appDirFile;
        }

        return Path.GetFullPath(SaveFileName);
    }

    public static bool SaveExists() => File.Exists(GetSavePath());

    public static void Save(Player player)
    {
        player.SaveDate = DateTime.Now;

        string[] lines =
        [
            "=== SAUVEGARDE HOW TO DATE JULES ===",
            $"Nom: {player.Name}",
            $"Age: {player.Age}",
            $"Sexe: {player.Gender}",
            $"Date: {player.SaveDate:yyyy-MM-dd HH:mm:ss}"
        ];

        File.WriteAllLines(GetSavePath(), lines);
    }

    public static Player? Load()
    {
        string path = GetSavePath();
        if (!File.Exists(path)) return null;

        try
        {
            var lines = File.ReadAllLines(path);
            var player = new Player();

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("=")) continue;

                int colonIndex = line.IndexOf(':');
                if (colonIndex < 0) continue;

                string key = line[..colonIndex].Trim('\uFEFF', ' ', '\t', '\r', '\n');
                string value = line[(colonIndex + 1)..].Trim('\uFEFF', ' ', '\t', '\r', '\n');

                switch (key.ToLowerInvariant())
                {
                    case "nom":
                        player.Name = value;
                        break;
                    case "age":
                        if (int.TryParse(value, out int age)) player.Age = age;
                        break;
                    case "sexe":
                        player.Gender = value;
                        break;
                    case "date":
                        if (DateTime.TryParse(value, out DateTime dt)) player.SaveDate = dt;
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(player.Name)) return null;

            return player;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Erreur] Impossible de lire la sauvegarde : {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }
}
