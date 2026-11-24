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
Typewriter.WriteLine("                ♥ ~ Le simulateur de séduction officiel ~ ♥\n", delayMs: 20);
Console.ResetColor();

Player? player = null;

// 1. Vérification si une sauvegarde existe déjà
if (SaveManager.SaveExists())
{
    var existingSave = SaveManager.Load();
    if (existingSave != null)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Typewriter.WriteLine("[!] Une sauvegarde existante a été trouvée :");
        Console.ForegroundColor = ConsoleColor.White;
        Typewriter.WriteLine($"    -> Joueur : {existingSave.Name}", delayMs: 10);
        Typewriter.WriteLine($"    -> Âge    : {existingSave.Age} ans", delayMs: 10);
        Typewriter.WriteLine($"    -> Sexe   : {existingSave.Gender}", delayMs: 10);
        Typewriter.WriteLine($"    -> Amour  : {existingSave.Affection}% ({ScenarioManager.GetStatusAffection(existingSave.Affection)})", delayMs: 10);
        Typewriter.WriteLine($"    -> Date   : {existingSave.SaveDate:dd/MM/yyyy HH:mm}\n", delayMs: 10);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Typewriter.Write(">> Voulez-vous charger cette sauvegarde ? (O/n) : ", delayMs: 10);
        Console.ForegroundColor = ConsoleColor.White;
        string? reponse = ReadCleanLine()?.ToLowerInvariant();

        if (string.IsNullOrEmpty(reponse) || reponse is "o" or "oui" or "y" or "yes")
        {
            player = existingSave;
            Console.ForegroundColor = ConsoleColor.Green;
            Typewriter.WriteLine("\n[✓] Sauvegarde chargée avec succès !");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Typewriter.WriteLine("\nCréation d'une nouvelle partie...\n", delayMs: 15);
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
    Typewriter.WriteLine($"\n[✓] Profil sauvegardé avec succès dans '{Path.GetFileName(SaveManager.GetSavePath())}' !");
    Console.ResetColor();
}

// 3. Début de l'aventure
Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("\n========================================================");
Console.ForegroundColor = ConsoleColor.Green;
Typewriter.WriteLine($"[!] Bienvenue dans l'aventure, {player.Name} ({player.Age} ans, {player.Gender}) !");
Typewriter.WriteLine("Jules est déjà sous le charme...");
Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("========================================================\n");
Console.ResetColor();

Typewriter.WriteLine("Appuyez sur n'importe quelle touche pour lancer le scénario...", delayMs: 15);
if (!Console.IsInputRedirected)
{
    Console.ReadKey(intercept: true);
}

// 4. Lecture et exécution du scénario depuis "situation.txt"
ScenarioManager.JouerScenario(player);

Console.ForegroundColor = ConsoleColor.Magenta;
Typewriter.WriteLine("\nMerci d'avoir joué à 'How To Date Jules' !");
Console.ResetColor();
Typewriter.WriteLine("Appuyez sur une touche pour quitter...", delayMs: 15);
if (!Console.IsInputRedirected)
{
    Console.ReadKey(intercept: true);
}

static string? ReadCleanLine()
{
    Typewriter.FlushInput();
    return Console.ReadLine()?.Trim('\uFEFF', ' ', '\t', '\r', '\n');
}

static Player CreerNouveauJoueur()
{
    var player = new Player();

    Console.ForegroundColor = ConsoleColor.Cyan;
    Typewriter.WriteLine("=== CRÉATION DU JOUEUR ===");
    Console.ResetColor();

    // 1. Saisie du nom
    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Typewriter.Write(">> Entrez votre nom de joueur : ", delayMs: 15);
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
        Typewriter.WriteLine("[X] Le nom ne peut pas être vide. Veuillez réessayer.\n", delayMs: 10);
        Console.ResetColor();
    }

    // 2. Saisie de l'âge
    bool tentativeMineur = false;
    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Typewriter.Write(">> Entrez votre âge : ", delayMs: 15);
        Console.ForegroundColor = ConsoleColor.White;

        string? inputAge = ReadCleanLine();
        if (inputAge == null && Console.IsInputRedirected)
        {
            player.Age = 20;
            Console.WriteLine(player.Age);
            break;
        }

        if (int.TryParse(inputAge, out int age))
        {
            if (age < 18)
            {
                if (!tentativeMineur)
                {
                    tentativeMineur = true;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Typewriter.WriteLine("Je vais appeler les flics , le beau Jules ne fréquente pas les mineurs\n", delayMs: 15);
                    Console.ResetColor();
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Magenta;
                Typewriter.WriteLine("Oh après tout l'age n'est qu'un concept moderne établie par des gens trop peu ouvert d'esprit\n", delayMs: 15);
                Console.ResetColor();
                player.Age = age;
                break;
            }

            if (age <= 120)
            {
                player.Age = age;
                break;
            }
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Typewriter.WriteLine("[X] Veuillez entrer un âge valide (ex: 20).\n", delayMs: 10);
        Console.ResetColor();
    }

    // 3. Saisie du sexe (homme, femme, ou non-binaire)
    Console.ForegroundColor = ConsoleColor.Yellow;
    Typewriter.WriteLine(">> Choisissez votre sexe :", delayMs: 15);
    Typewriter.WriteLine("   [1] Homme", delayMs: 10);
    Typewriter.WriteLine("   [2] Femme", delayMs: 10);
    Typewriter.WriteLine("   [3] Non-binaire", delayMs: 10);

    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Typewriter.Write(">> Votre choix (1, 2 ou 3) : ", delayMs: 15);
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
        Typewriter.WriteLine("[X] Choix invalide. Veuillez taper 1 (Homme), 2 (Femme) ou 3 (Non-binaire).\n", delayMs: 10);
        Console.ResetColor();
    }

    return player;
}
