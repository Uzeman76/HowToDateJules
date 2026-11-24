// ==========================================
//          HOW TO DATE JULES
// ==========================================

Console.Title = "How To Date Jules";
Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine(@"
   __  __              ______         ____        __           __      __          
  / / / /___ _      __/_  __/___     / __ \____ _/ /____      / /_  __/ /__  _____ 
 / /_/ / __ \ | /| / / / / / __ \   / / / / __ `/ __/ _ \__  / / / / / / _ \/ ___/ 
/ __  / /_/ / |/ |/ / / / / /_/ /  / /_/ / /_/ / /_/  __/ /_/ / /_/ / /  __(__  )  
\/ /_/\____/|__/|__/ /_/  \____/  /_____/\__,_/\__/\___/\____/\__,_/_/\___/____/   
");

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("                ♥ ~ Le simulateur de séduction officiel ~ ♥\n");

Console.ResetColor();
Console.ForegroundColor = ConsoleColor.Yellow;
Console.Write(">> Entrez votre nom de joueur : ");
Console.ForegroundColor = ConsoleColor.White;

string? playerName = Console.ReadLine()?.Trim();
if (string.IsNullOrWhiteSpace(playerName))
{
    playerName = "Mystérieux(se) Inconnu(e)";
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"\n[!] Bienvenue dans l'aventure, {playerName} ! Jules est déjà sous le charme...");
Console.ResetColor();

Console.WriteLine("\nAppuyez sur n'importe quelle touche pour commencer...");
Console.ReadKey(intercept: true);