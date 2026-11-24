namespace ConsoleApp2;

public static class ScenarioManager
{
    public const string DefaultFileName = "situation.txt";

    /// <summary>
    /// Trouve le chemin absolu du fichier de situation ("situation.txt" ou "situation").
    /// </summary>
    public static string GetScenarioFilePath()
    {
        // 1. Répertoire de travail courant
        if (File.Exists(DefaultFileName)) return Path.GetFullPath(DefaultFileName);
        if (File.Exists("situation")) return Path.GetFullPath("situation");

        // 2. Répertoire du sous-dossier ConsoleApp2 (si exécuté depuis la racine du repo)
        string subfolderFile = Path.Combine("ConsoleApp2", DefaultFileName);
        if (File.Exists(subfolderFile)) return Path.GetFullPath(subfolderFile);

        // 3. Répertoire de base de l'application (bin/Debug/...)
        string appDirFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultFileName);
        if (File.Exists(appDirFile)) return appDirFile;

        // 4. Répertoire racine du projet source
        string projectSourceFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", DefaultFileName);
        if (File.Exists(projectSourceFile)) return Path.GetFullPath(projectSourceFile);

        return Path.GetFullPath(DefaultFileName);
    }

    /// <summary>
    /// Lit le fichier "situation" et extrait l'ensemble des scènes, textes et choix avec modificateurs d'affection.
    /// </summary>
    public static Dictionary<string, Situation> LireFichierSituation(string cheminFichier)
    {
        var situations = new Dictionary<string, Situation>(StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(cheminFichier))
        {
            return situations;
        }

        var lines = File.ReadAllLines(cheminFichier);
        Situation? currentSituation = null;
        var textBuffer = new List<string>();
        string currentSection = "";

        void FinaliserSituationCourante()
        {
            if (currentSituation != null)
            {
                currentSituation.Text = string.Join(Environment.NewLine, textBuffer).Trim();
                textBuffer.Clear();
                if (!string.IsNullOrWhiteSpace(currentSituation.Id))
                {
                    situations[currentSituation.Id] = currentSituation;
                }
            }
        }

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim('\uFEFF', '\r');
            var trimmed = line.Trim();

            // Détection d'un en-tête : "=== SITUATION 1 ===" ou "[SITUATION: 1]"
            if (trimmed.StartsWith("===") || trimmed.StartsWith("[SITUATION", StringComparison.OrdinalIgnoreCase))
            {
                FinaliserSituationCourante();

                string id = ExtraireIdSituation(trimmed);
                currentSituation = new Situation { Id = id };
                currentSection = "HEADER";
                continue;
            }

            if (currentSituation == null) continue;

            if (trimmed.StartsWith("Titre:", StringComparison.OrdinalIgnoreCase))
            {
                currentSituation.Title = trimmed[6..].Trim();
                continue;
            }

            if (trimmed.Equals("Texte:", StringComparison.OrdinalIgnoreCase))
            {
                currentSection = "TEXT";
                continue;
            }

            if (trimmed.StartsWith("Fin:", StringComparison.OrdinalIgnoreCase))
            {
                currentSituation.EndingMessage = trimmed[4..].Trim();
                currentSection = "END";
                continue;
            }

            if (trimmed.StartsWith("Choix:", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("Choix", StringComparison.OrdinalIgnoreCase))
            {
                currentSection = "CHOICES";
                continue;
            }

            if (currentSection == "TEXT")
            {
                textBuffer.Add(line);
            }
            else if (currentSection == "CHOICES")
            {
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                var choice = ParserLigneChoix(trimmed, currentSituation.Choices.Count + 1);
                if (choice != null)
                {
                    currentSituation.Choices.Add(choice);
                }
            }
        }

        FinaliserSituationCourante();
        return situations;
    }

    private static string ExtraireIdSituation(string header)
    {
        var clean = header.Replace("=", "").Replace("[", "").Replace("]", "");
        int index = clean.IndexOf("SITUATION", StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            clean = clean[(index + "SITUATION".Length)..];
        }
        clean = clean.Replace(":", "").Trim();
        return string.IsNullOrEmpty(clean) ? "1" : clean;
    }

    private static Choice? ParserLigneChoix(string line, int defaultNumber)
    {
        // Formats acceptés :
        // 1 | Texte du choix -> ID | +15
        // 1 | Texte du choix -> ID [+15]
        // 1 | Texte du choix -> ID
        string textPart = line;
        string targetPart = "";
        int affectionChange = 0;

        int arrowIndex = line.IndexOf("->");
        if (arrowIndex >= 0)
        {
            textPart = line[..arrowIndex].Trim();
            targetPart = line[(arrowIndex + 2)..].Trim();
        }

        // 1. Extraction d'un modificateur d'amour : format "| +15" ou "| -10"
        int secondPipe = targetPart.IndexOf('|');
        if (secondPipe >= 0)
        {
            string affRaw = targetPart[(secondPipe + 1)..].Replace("+", "").Trim();
            if (int.TryParse(affRaw, out int parsedAff))
            {
                affectionChange = parsedAff;
            }
            targetPart = targetPart[..secondPipe].Trim();
        }

        // 2. Extraction d'un modificateur d'amour : format "[+15]" ou "[-10]"
        int bracketStart = targetPart.IndexOf('[');
        int bracketEnd = targetPart.IndexOf(']');
        if (bracketStart >= 0 && bracketEnd > bracketStart)
        {
            string affRaw = targetPart.Substring(bracketStart + 1, bracketEnd - bracketStart - 1).Replace("+", "").Trim();
            if (int.TryParse(affRaw, out int parsedAff))
            {
                affectionChange = parsedAff;
            }
            targetPart = targetPart[..bracketStart].Trim();
        }

        int number = defaultNumber;
        int pipeIndex = textPart.IndexOf('|');
        if (pipeIndex >= 0)
        {
            string numStr = textPart[..pipeIndex].Trim();
            if (int.TryParse(numStr, out int parsedNum))
            {
                number = parsedNum;
            }
            textPart = textPart[(pipeIndex + 1)..].Trim();
        }
        else
        {
            int dotIndex = textPart.IndexOf('.');
            if (dotIndex > 0 && int.TryParse(textPart[..dotIndex].Trim(), out int parsedDotNum))
            {
                number = parsedDotNum;
                textPart = textPart[(dotIndex + 1)..].Trim();
            }
        }

        return new Choice
        {
            Number = number,
            Text = textPart,
            TargetSituationId = targetPart,
            AffectionChange = affectionChange
        };
    }

    /// <summary>
    /// Affiche graphiquement la jauge d'amour de Jules sous forme de cœurs et pourcentage.
    /// </summary>
    public static void AfficherJaugeAmour(int affection)
    {
        int totalHearts = 10;
        int filledHearts = (int)Math.Round((affection / 100.0) * totalHearts);
        filledHearts = Math.Clamp(filledHearts, 0, totalHearts);
        int emptyHearts = totalHearts - filledHearts;

        string full = new string('♥', filledHearts);
        string empty = new string('♡', emptyHearts);
        string status = GetStatusAffection(affection);

        Console.Write("  Jauge d'Amour : [ ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write(full);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(empty);
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write($" ] {affection}% ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"({status})");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static string GetStatusAffection(int affection)
    {
        return affection switch
        {
            < 20 => "❄️ Froid & Distant",
            < 40 => "💭 Intrigué",
            < 60 => "😊 Complice",
            < 80 => "💖 Coup de cœur",
            _ => "💘 Fou amoureux !"
        };
    }

    /// <summary>
    /// Exécute et affiche le scénario interactivement à l'écran avec jauge d'amour dynamique.
    /// </summary>
    public static void JouerScenario(Player player, string? cheminFichier = null)
    {
        string path = cheminFichier ?? GetScenarioFilePath();

        if (!File.Exists(path))
        {
            CreerFichierSituationParDefaut(path);
        }

        var situations = LireFichierSituation(path);
        if (situations.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Typewriter.WriteLine($"[Erreur] Aucune situation valide trouvée dans '{path}'.");
            Console.ResetColor();
            return;
        }

        string currentId = "1";
        if (!situations.ContainsKey(currentId))
        {
            currentId = situations.Keys.First();
        }

        while (true)
        {
            if (!situations.TryGetValue(currentId, out var situation))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Typewriter.WriteLine($"[Erreur] La situation '{currentId}' n'existe pas dans le scénario.");
                Console.ResetColor();
                break;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("========================================================");
            if (!string.IsNullOrWhiteSpace(situation.Title))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Typewriter.WriteLine($"  ♥ {RemplacerVariables(situation.Title, player)} ♥", delayMs: 15);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("--------------------------------------------------------");
            }

            // Affichage de la jauge d'amour de Jules
            AfficherJaugeAmour(player.Affection);

            // Affichage du texte du scénario avec effet rétro
            Console.ForegroundColor = ConsoleColor.White;
            string texteFormate = RemplacerVariables(situation.Text, player);
            Typewriter.WriteLine(texteFormate);
            Console.WriteLine();

            // Si la situation est une fin
            if (situation.IsEnding)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                if (!string.IsNullOrWhiteSpace(situation.EndingMessage))
                {
                    Typewriter.WriteLine($"[FIN] {RemplacerVariables(situation.EndingMessage, player)}\n");
                }
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("========================================================");
                Console.ResetColor();
                break;
            }

            // Affichage des choix
            Console.ForegroundColor = ConsoleColor.Yellow;
            Typewriter.WriteLine("Que décides-tu de faire ?", delayMs: 15);
            foreach (var choice in situation.Choices)
            {
                Typewriter.WriteLine($"   [{choice.Number}] {RemplacerVariables(choice.Text, player)}", delayMs: 15);
            }
            Console.WriteLine();

            // Saisie du choix
            while (true)
            {
                Typewriter.FlushInput();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Typewriter.Write(">> Ton choix : ", delayMs: 10);
                Console.ForegroundColor = ConsoleColor.White;

                string? saisie = Console.ReadLine()?.Trim('\uFEFF', ' ', '\t', '\r', '\n');
                if (saisie == null && Console.IsInputRedirected)
                {
                    saisie = "1";
                    Console.WriteLine(saisie);
                }

                if (int.TryParse(saisie, out int choixNum))
                {
                    var choixSelectionne = situation.Choices.FirstOrDefault(c => c.Number == choixNum);
                    if (choixSelectionne != null)
                    {
                        // Impact sur la jauge d'amour de Jules
                        if (choixSelectionne.AffectionChange != 0)
                        {
                            player.Affection = Math.Clamp(player.Affection + choixSelectionne.AffectionChange, 0, 100);
                            SaveManager.Save(player); // Sauvegarde automatique du nouveau score

                            if (choixSelectionne.AffectionChange > 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Typewriter.WriteLine($"\n[♥ Jules a adoré cette réponse ! (+{choixSelectionne.AffectionChange}% d'amour)]\n", delayMs: 10);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Typewriter.WriteLine($"\n[💔 Jules n'a pas trop aimé cette réponse... ({choixSelectionne.AffectionChange}% d'amour)]\n", delayMs: 10);
                            }
                            Console.ResetColor();

                            if (!Console.IsInputRedirected)
                            {
                                Thread.Sleep(300);
                            }
                        }

                        currentId = choixSelectionne.TargetSituationId;
                        break;
                    }
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Typewriter.WriteLine("[X] Choix invalide. Tape le numéro correspondant à ton choix.\n", delayMs: 10);
                Console.ResetColor();
            }
        }
    }

    public static string RemplacerVariables(string template, Player player)
    {
        return template
            .Replace("{nom}", player.Name, StringComparison.OrdinalIgnoreCase)
            .Replace("{joueur}", player.Name, StringComparison.OrdinalIgnoreCase)
            .Replace("{name}", player.Name, StringComparison.OrdinalIgnoreCase)
            .Replace("{age}", player.Age.ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{sexe}", player.Gender, StringComparison.OrdinalIgnoreCase)
            .Replace("{amour}", player.Affection.ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{affection}", player.Affection.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public static void CreerFichierSituationParDefaut(string path)
    {
        string contenu = @"=== SITUATION 1 ===
Titre: Le café renversé
Texte:
Tu marches tranquillement dans le parc quand soudain, quelqu'un te bouscule doucement.
Un grand jeune homme aux yeux rieurs et aux cheveux châtains rattrape ton gobelet in extremis !
C'est Jules !
""Ouf, sauvé(e) ! Tu t'appelles bien {nom}, non ? Je t'ai déjà croisé(e) à la fac...""
Choix:
1 | ""Oui c'est bien moi ! Merci pour le réflexe, Jules."" -> 2 | +15
2 | ""Un vrai ninja ! Tu sauves des cafés toute la journée ?"" -> 3 | +10
3 | ""Attention où tu marches quand même..."" -> 4 | -20

=== SITUATION 2 ===
Titre: Le charme opère
Texte:
Jules sourit de toutes ses dents, visiblement ravi d'engager la conversation avec toi.
""Tout le plaisir est pour moi, {nom}. Dis, pour me faire pardonner la frayeur, tu accepterais de prendre un vrai café avec moi ?""
Choix:
1 | ""J'accepte avec grand plaisir !"" -> 5 | +20
2 | ""Seulement si c'est toi qui choisis la pâtisserie."" -> 5 | +15

=== SITUATION 3 ===
Titre: La touche d'humour
Texte:
Jules éclate de rire et passe une main dans ses cheveux :
""Seulement quand une personne aussi charmante passe devant moi ! Plus sérieusement, tu as un peu de temps pour papoter ?""
Choix:
1 | ""Pour toi Jules, j'ai tout mon temps !"" -> 5 | +25
2 | ""Juste quelques minutes alors..."" -> 5 | +10

=== SITUATION 4 ===
Titre: Trop distant(e)
Texte:
Jules perd son sourire et baisse les yeux, visiblement refroidi par ta réponse.
""Oh... Pardon, je ne voulais pas t'embêter. Bonne journée {nom}.""
Il tourne les talons et disparaît dans la foule.
Fin: GAME OVER - Tu as été trop froid(e)... Affection finale avec Jules : {amour}%.

=== SITUATION 5 ===
Titre: Le rendez-vous au café
Texte:
Vous vous installez à la terrasse ensoleillée d'un petit café cosy.
Jules te regarde avec attention et demande :
""Alors {nom}, dis-moi ce qui te fait vibrer dans la vie ?""
Choix:
1 | Lui parler de tes passions avec des étoiles dans les yeux -> 6 | +20
2 | Lui faire un compliment sincère sur son style -> 6 | +15
3 | Regarder ton téléphone et répondre d'un air distrait -> 6 | -20

=== SITUATION 6 ===
Titre: Le dénouement romantique
Texte:
Le soleil commence à se coucher. Jules pose délicatement sa main près de la tienne.
Ses joues prennent une jolie teinte rosée.
""J'ai passé un moment absolument magique avec toi, {nom}...""
Fin: BILAN AMOUREUX ♥ - Ton score d'amour avec Jules atteint {amour}% ! Jules est conquis !
";
        File.WriteAllText(path, contenu, System.Text.Encoding.UTF8);
    }
}
