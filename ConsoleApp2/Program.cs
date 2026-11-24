// ==========================================
//          HOW TO DATE JULES
// ==========================================

using ConsoleApp2;

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

Player? player = null;

// 1. Vérification si une sauvegarde existe déjà
if (SaveManager.SaveExists())
{
    var existingSave = SaveManager.Load();
    if (existingSave != null)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[!] Une sauvegarde existante a été trouvée :");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"    -> Joueur : {existingSave.Name}");
        Console.WriteLine($"    -> Âge    : {existingSave.Age} ans");
        Console.WriteLine($"    -> Sexe   : {existingSave.Gender}");
        Console.WriteLine($"    -> Date   : {existingSave.SaveDate:dd/MM/yyyy HH:mm}");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(">> Voulez-vous charger cette sauvegarde ? (O/n) : ");
        Console.ForegroundColor = ConsoleColor.White;
        string? reponse = ReadCleanLine()?.ToLowerInvariant();

        if (string.IsNullOrEmpty(reponse) || reponse is "o" or "oui" or "y" or "yes")
        {
            player = existingSave;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[✓] Sauvegarde chargée avec succès !");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nCréation d'une nouvelle partie...\n");
            Console.ResetColor();
        }
    }
}

// 2. Si aucune sauvegarde ou si le joueur veut recommencer
if (player == null)
{
    player = CreerNouveauJoueur();
    SaveManager.Save(player);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n[✓] Profil sauvegardé avec succès dans '{Path.GetFileName(SaveManager.GetSavePath())}' !");
    Console.ResetColor();
}

// 3. Début de l'aventure
Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("\n========================================================");
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"[!] Bienvenue dans l'aventure, {player.Name} ({player.Age} ans, {player.Gender}) !");
Console.WriteLine("Jules est déjà sous le charme...");
Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("========================================================\n");
Console.ResetColor();

Console.WriteLine("Appuyez sur n'importe quelle touche pour lancer le scénario...");
if (!Console.IsInputRedirected)
{
    Console.ReadKey(intercept: true);
}

// 4. Lecture et exécution du scénario depuis "situation.txt"
ScenarioManager.JouerScenario(player);

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("\nMerci d'avoir joué à 'How To Date Jules' !");
Console.ResetColor();
Console.WriteLine("Appuyez sur une touche pour quitter...");
if (!Console.IsInputRedirected)
{
    Console.ReadKey(intercept: true);
}

static string? ReadCleanLine()
{
    return Console.ReadLine()?.Trim('\uFEFF', ' ', '\t', '\r', '\n');
}

static Player CreerNouveauJoueur()
{
    var player = new Player();

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("=== CRÉATION DU JOUEUR ===");
    Console.ResetColor();

    // 1. Saisie du nom
    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(">> Entrez votre nom de joueur : ");
        Console.ForegroundColor = ConsoleColor.White;

        string? inputName = ReadCleanLine();
        if (inputName == null && Console.IsInputRedirected)
        {
            player.Name = "Joueur";
            Console.WriteLine(player.Name);
            break;
        }

        if (!string.IsNullOrWhiteSpace(inputName))
        {
            player.Name = inputName;
            break;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[X] Le nom ne peut pas être vide. Veuillez réessayer.\n");
        Console.ResetColor();
    }

    // 2. Saisie de l'âge
    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(">> Entrez votre âge : ");
        Console.ForegroundColor = ConsoleColor.White;

        string? inputAge = ReadCleanLine();
        if (inputAge == null && Console.IsInputRedirected)
        {
            player.Age = 20;
            Console.WriteLine(player.Age);
            break;
        }

        if (int.TryParse(inputAge, out int age) && age is >= 1 and <= 120)
        {
            player.Age = age;
            break;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[X] Veuillez entrer un âge valide (ex: 20).\n");
        Console.ResetColor();
    }

    // 3. Saisie du sexe (homme, femme, ou non-binaire)
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(">> Choisissez votre sexe :");
    Console.WriteLine("   [1] Homme");
    Console.WriteLine("   [2] Femme");
    Console.WriteLine("   [3] Non-binaire");

    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(">> Votre choix (1, 2 ou 3) : ");
        Console.ForegroundColor = ConsoleColor.White;

        string? inputGender = ReadCleanLine()?.ToLowerInvariant();
        if (inputGender == null && Console.IsInputRedirected)
        {
            player.Gender = "Non-binaire";
            Console.WriteLine("3");
            break;
        }

        if (inputGender is "1" or "homme" or "h")
        {
            player.Gender = "Homme";
            break;
        }
        if (inputGender is "2" or "femme" or "f")
        {
            player.Gender = "Femme";
            break;
        }
        if (inputGender is "3" or "non-binaire" or "non binaire" or "nb")
        {
            player.Gender = "Non-binaire";
            break;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[X] Choix invalide. Veuillez taper 1 (Homme), 2 (Femme) ou 3 (Non-binaire).\n");
        Console.ResetColor();
    }

    return player;
}
