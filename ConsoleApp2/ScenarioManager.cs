namespace ConsoleApp2;

public static class ScenarioManager
{
    public const string DefaultFileName = "situation_parc.txt";

    /// <summary>
    /// Trouve le chemin absolu d'un fichier de situation donné (ou le premier par défaut).
    /// </summary>
    public static string GetScenarioFilePath(string? fileName = null)
    {
        string target = string.IsNullOrWhiteSpace(fileName) ? DefaultFileName : fileName;

        // 1. Répertoire courant
        if (File.Exists(target)) return Path.GetFullPath(target);

        // Si le nom par défaut "situation_parc.txt" n'existe pas, vérifier "situation.txt"
        if (target == DefaultFileName && File.Exists("situation.txt"))
        {
            return Path.GetFullPath("situation.txt");
        }

        // 2. Sous-dossier ConsoleApp2
        string subfolderFile = Path.Combine("ConsoleApp2", target);
        if (File.Exists(subfolderFile)) return Path.GetFullPath(subfolderFile);

        // 3. Répertoire de l'exécutable (bin/Debug/...)
        string appDirFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, target);
        if (File.Exists(appDirFile)) return appDirFile;

        // 4. Répertoire racine du projet source
        string projectSourceFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", target);
        if (File.Exists(projectSourceFile)) return Path.GetFullPath(projectSourceFile);

        return Path.GetFullPath(target);
    }

    /// <summary>
    /// Lit et analyse un fichier de scénario/date.
    /// </summary>
    public static ScenarioData LireFichierScenario(string cheminFichier)
    {
        var data = new ScenarioData();

        if (!File.Exists(cheminFichier))
        {
            return data;
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
                    data.Situations[currentSituation.Id] = currentSituation;
                }
            }
        }

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim('\uFEFF', '\r');
            var trimmed = line.Trim();

            // Méta-données globales du date en début de fichier
            if (currentSituation == null)
            {
                if (trimmed.StartsWith("TITRE_DATE:", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("DATE:", StringComparison.OrdinalIgnoreCase))
                {
                    data.DateTitle = trimmed[(trimmed.IndexOf(':') + 1)..].Trim();
                    continue;
                }
                if (trimmed.StartsWith("SUIVANT:", StringComparison.OrdinalIgnoreCase))
                {
                    data.DefaultNextScenario = trimmed[(trimmed.IndexOf(':') + 1)..].Trim();
                    continue;
                }
            }

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

            if (trimmed.StartsWith("Suivant:", StringComparison.OrdinalIgnoreCase))
            {
                currentSituation.NextScenarioFile = trimmed[8..].Trim();
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
        return data;
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
        string textPart = line;
        string targetPart = "";
        int affectionChange = 0;

        int arrowIndex = line.IndexOf("->");
        if (arrowIndex >= 0)
        {
            textPart = line[..arrowIndex].Trim();
            targetPart = line[(arrowIndex + 2)..].Trim();
        }

        // Format | +15 ou | -10
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

        // Format [+15] ou [-10]
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

        textPart = textPart.Trim('\"', ' ');

        return new Choice
        {
            Number = number,
            Text = textPart,
            TargetSituationId = targetPart,
            AffectionChange = affectionChange
        };
    }

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
    /// Affiche le récapitulatif détaillé d'un date avec les bons et mauvais choix.
    /// </summary>
    public static void AfficherRecapitulatifDate(Player player, string dateTitle, List<ChoiceRecord> choixHistorique, int affectionDepart)
    {
        int delta = player.Affection - affectionDepart;
        int bonsChoix = choixHistorique.Count(c => c.EstBonChoix);
        int mauvaisChoix = choixHistorique.Count(c => c.EstMauvaisChoix);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════════╗");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"║          ♥ RÉCAPITULATIF DU RENDEZ-VOUS : {dateTitle.ToUpper()} ♥");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Typewriter.WriteLine("\nVos choix durant ce rendez-vous :");
        Console.ResetColor();

        if (choixHistorique.Count == 0)
        {
            Console.WriteLine("  (Aucun choix spécifique enregistré)");
        }
        else
        {
            foreach (var rec in choixHistorique)
            {
                if (rec.EstBonChoix)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("  [✓ BON CHOIX]     ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"\"{RemplacerVariables(rec.ChoiceText, player)}\" ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"(+{rec.AffectionChange}% ♥)");
                }
                else if (rec.EstMauvaisChoix)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("  [✗ MAUVAIS CHOIX] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"\"{RemplacerVariables(rec.ChoiceText, player)}\" ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"({rec.AffectionChange}% 💔)");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("  [~ NEUTRE]        ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"\"{RemplacerVariables(rec.ChoiceText, player)}\" (0%)");
                }
            }
        }

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\n----------------------------------------------------------------------");
        Console.ForegroundColor = ConsoleColor.Yellow;
        string signe = delta >= 0 ? "+" : "";
        Console.WriteLine($"  Bilan du rendez-vous : {signe}{delta}% d'amour ({bonsChoix} bon(s) choix, {mauvaisChoix} mauvais choix)");

        AfficherJaugeAmour(player.Affection);

        if (delta > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Typewriter.WriteLine("  ♥ Jules repart le cœur battant, visiblement sous votre charme ! ♥\n", delayMs: 15);
        }
        else if (delta < 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Typewriter.WriteLine("  💔 Jules semble un peu perplexe ou blessé par certaines de vos réponses... 💔\n", delayMs: 15);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Typewriter.WriteLine("  ~ Un date agréable, mais Jules attend encore d'être surpris ! ~\n", delayMs: 15);
        }
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("----------------------------------------------------------------------\n");
        Console.ResetColor();
    }

    /// <summary>
    /// Exécute une série de rendez-vous / situations avec récapitulatif entre chaque lieu.
    /// </summary>
    public static void JouerScenario(Player player, string? cheminInitial = null)
    {
        VerifierFichiersScenariosParDefaut();

        string? scenarioCourant = cheminInitial ?? player.CurrentScenario;

        while (!string.IsNullOrWhiteSpace(scenarioCourant))
        {
            string path = GetScenarioFilePath(scenarioCourant);

            if (!File.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Typewriter.WriteLine($"[Erreur] Impossible de trouver le scénario '{scenarioCourant}'.\n");
                Console.ResetColor();
                break;
            }

            var scenarioData = LireFichierScenario(path);
            if (scenarioData.Situations.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Typewriter.WriteLine($"[Erreur] Aucune situation valide dans '{path}'.\n");
                Console.ResetColor();
                break;
            }

            int affectionDepart = player.Affection;
            var choixHistorique = new List<ChoiceRecord>();
            string currentId = "1";

            // Si le joueur a une sauvegarde au milieu de ce scénario, reprendre à cet endroit précis
            if (!string.IsNullOrWhiteSpace(player.CurrentSituationId) && scenarioData.Situations.ContainsKey(player.CurrentSituationId))
            {
                currentId = player.CurrentSituationId;
                if (currentId != "1")
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Typewriter.WriteLine($"\n[✓] Reprise automatique de votre partie : {scenarioData.DateTitle} (Scène {currentId}) !\n", delayMs: 10);
                    Console.ResetColor();
                }
            }
            else if (!scenarioData.Situations.ContainsKey(currentId))
            {
                currentId = scenarioData.Situations.Keys.First();
            }

            string? prochainFichier = scenarioData.DefaultNextScenario;

            // Boucle des scènes de ce date
            while (true)
            {
                if (!scenarioData.Situations.TryGetValue(currentId, out var situation))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Typewriter.WriteLine($"[Erreur] La situation '{currentId}' n'existe pas.\n");
                    Console.ResetColor();
                    break;
                }

                // Sauvegarder immédiatement la progression sur la scène courante
                player.CurrentScenario = Path.GetFileName(path);
                player.CurrentSituationId = currentId;
                player.CurrentDateTitle = scenarioData.DateTitle;
                SaveManager.Save(player);

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

                AfficherJaugeAmour(player.Affection);

                Console.ForegroundColor = ConsoleColor.White;
                string texteFormate = RemplacerVariables(situation.Text, player);
                Typewriter.WriteLine(texteFormate);
                Console.WriteLine();

                // Si la scène est une fin de date
                if (situation.IsEnding)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    if (!string.IsNullOrWhiteSpace(situation.EndingMessage))
                    {
                        Typewriter.WriteLine($"[FIN DU LIEU] {RemplacerVariables(situation.EndingMessage, player)}\n");
                    }
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("========================================================");
                    Console.ResetColor();

                    if (!string.IsNullOrWhiteSpace(situation.NextScenarioFile))
                    {
                        prochainFichier = situation.NextScenarioFile;
                    }

                    player.CurrentSituationId = currentId;
                    SaveManager.Save(player);
                    break;
                }

                // Choix possibles
                Console.ForegroundColor = ConsoleColor.Yellow;
                Typewriter.WriteLine("Que décides-tu de faire ?", delayMs: 15);
                foreach (var choice in situation.Choices)
                {
                    Typewriter.WriteLine($"   [{choice.Number}] {RemplacerVariables(choice.Text, player)}", delayMs: 15);
                }
                Console.WriteLine();

                // Saisie
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
                            // Enregistrer le choix dans l'historique pour le récapitulatif
                            choixHistorique.Add(new ChoiceRecord
                            {
                                SceneTitle = situation.Title,
                                ChoiceText = choixSelectionne.Text,
                                AffectionChange = choixSelectionne.AffectionChange
                            });

                            // Impact d'affection
                            if (choixSelectionne.AffectionChange != 0)
                            {
                                player.Affection = Math.Clamp(player.Affection + choixSelectionne.AffectionChange, 0, 100);

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

            // AFFICHER LE RÉCAPITULATIF DU DATE
            AfficherRecapitulatifDate(player, scenarioData.DateTitle, choixHistorique, affectionDepart);

            // Passage au date suivant ou fin
            if (!string.IsNullOrWhiteSpace(prochainFichier))
            {
                player.CurrentScenario = prochainFichier;
                player.CurrentSituationId = "1"; // Le prochain lieu commence à la scène 1
                SaveManager.Save(player);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Typewriter.WriteLine("Appuyez sur n'importe quelle touche pour passer au prochain lieu...\n", delayMs: 15);
                Console.ResetColor();
                if (!Console.IsInputRedirected)
                {
                    Console.ReadKey(intercept: true);
                }
                scenarioCourant = prochainFichier;
            }
            else
            {
                // Plus d'autres dates : toute l'aventure est terminée !
                player.EstJeuTermine = true;
                player.CurrentSituationId = currentId;
                SaveManager.Save(player);
                scenarioCourant = null;
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

    public static void VerifierFichiersScenariosParDefaut()
    {
        string parcPath = GetScenarioFilePath("situation_parc.txt");
        if (!File.Exists(parcPath))
        {
            CreerFichierParc(parcPath);
        }

        string ecolePath = GetScenarioFilePath("situation_ecole.txt");
        if (!File.Exists(ecolePath))
        {
            CreerFichierEcole(ecolePath);
        }
    }

    public static void CreerFichierParc(string path)
    {
        string contenu = @"TITRE_DATE: Le Date au Parc
SUIVANT: situation_ecole.txt

=== SITUATION 1 ===
Titre: Le café renversé
Texte:
Tu te balades tranquillement dans les allées fleuries du parc quand quelqu'un te bouscule doucement.
Un grand jeune homme aux yeux rieurs rattrape ton gobelet in extremis !
C'est Jules !
""Ouf sauvé(e) ! Tu t'appelles bien {nom}, non ? Je t'ai déjà croisé(e)...""
Choix:
1 | ""Oui c'est bien moi ! Merci pour le réflexe, Jules."" -> 2 | +15
2 | ""Un vrai ninja ! Tu sauves des cafés toute la journée ?"" -> 3 | +10
3 | ""Attention où tu marches quand même..."" -> 4 | -20

=== SITUATION 2 ===
Titre: Le stand de glaces
Texte:
Jules sourit de toutes ses dents :
""Tout le plaisir est pour moi, {nom} ! Dis, pour me faire pardonner, on va chercher une glace au stand du parc ?""
Choix:
1 | ""Avec grand plaisir, je choisis le parfum !"" -> 5 | +20
2 | ""Seulement si c'est toi qui régales."" -> 5 | +10
3 | ""Non merci, je fais attention à ma ligne."" -> 5 | -10

=== SITUATION 3 ===
Titre: La touche d'humour
Texte:
Jules éclate de rire et passe une main dans ses cheveux :
""Seulement quand une personne aussi charmante passe devant moi ! Tu as un peu de temps pour te promener avec moi près du lac ?""
Choix:
1 | ""Pour toi Jules, j'ai tout mon temps !"" -> 5 | +25
2 | ""Juste quelques minutes alors..."" -> 5 | +10
3 | ""Bof, marcher ça fatigue."" -> 4 | -15

=== SITUATION 4 ===
Titre: Ambiance glaciale
Texte:
Jules perd son sourire et semble blessé par ta froideur.
""Oh... Pardon de t'avoir dérangé(e) {nom}. Bonne journée alors...""
Il tourne les talons et disparaît au loin.
Fin: Jules est reparti déçu... Mais l'aventure ne s'arrête pas là !
Suivant: situation_ecole.txt

=== SITUATION 5 ===
Titre: Fin d'après-midi au parc
Texte:
Vous marchez côte à côte en discutant de tout et de rien. La complicité est palpable.
À la fin de la promenade, Jules te sourit timidement :
""C'était super cette après-midi avec toi {nom}. On se retrouve demain à l'école ?""
Fin: Fin d'un merveilleux rendez-vous au parc !
Suivant: situation_ecole.txt
";
        File.WriteAllText(path, contenu, System.Text.Encoding.UTF8);
    }

    public static void CreerFichierEcole(string path)
    {
        string contenu = @"TITRE_DATE: Une Journée à l'École
SUIVANT: 

=== SITUATION 1 ===
Titre: Retrouvailles dans le couloir
Texte:
Le lendemain matin, tu arrives devant ton casier à l'école.
Jules t'attendait, appuyé contre le mur avec une briquette de jus pour toi !
""Coucou {nom} ! J'espérais tellement te croiser avant le premier cours...""
Choix:
1 | Lui faire un grand sourire et accepter le jus avec tendresse -> 2 | +20
2 | Rougir et le remercier timidement -> 2 | +10
3 | ""Encore toi ? Tu me suis partout ma parole !"" -> 3 | -20

=== SITUATION 2 ===
Titre: Le binôme de TP
Texte:
Pendant le cours de sciences, le professeur demande de se mettre en binôme pour une expérience.
Jules te fait un signe discret depuis l'autre bout de la salle en pointant la chaise à côté de lui.
Choix:
1 | Le rejoindre aussitôt avec enthousiasme -> 4 | +20
2 | Hésiter pour le faire mariner un peu avant de le rejoindre -> 4 | +5
3 | Choisir quelqu'un d'autre pour le rendre jaloux -> 5 | -25

=== SITUATION 3 ===
Titre: Malaise matinal
Texte:
Jules baisse les yeux, touché par ta remarque cassante.
""Désolé, je pensais juste te faire plaisir...""
Il range son jus et file en classe la tête basse.
Choix:
1 | Le rattraper par le bras pour t'excuser -> 4 | +10
2 | Hausser les épaules et aller en cours sans un mot -> 5 | -20

=== SITUATION 4 ===
Titre: Pause déjeuner sur le toit
Texte:
À midi, Jules t'invite sur le toit de l'école, un endroit secret et paisible.
Le vent souffle doucement dans ses cheveux.
""Tu sais {nom}... Depuis qu'on s'est croisés au parc, tu es tout le temps dans mes pensées.""
Choix:
1 | ""Moi aussi Jules, tu as volé mon cœur..."" -> 6 | +25
2 | ""Tu es vraiment un grand romantique Jules !"" -> 6 | +15
3 | ""Désolé(e), j'étais concentré(e) sur mon sandwich."" -> 5 | -20

=== SITUATION 5 ===
Titre: Distance et regrets
Texte:
L'ambiance est lourde et Jules garde ses distances toute la fin de la journée.
Il quitte l'établissement en silence dès que la sonnerie retentit.
Fin: FIN DE LA JOURNÉE - Jules est distant... Score d'amour final : {amour}%.

=== SITUATION 6 ===
Titre: La confession sous les cerisiers
Texte:
À la sortie des cours, Jules t'attend sous les cerisiers en fleurs devant la grille.
Il prend tes deux mains dans les siennes :
""{nom}, je ne veux plus passer une seule journée sans toi... Tu veux être avec moi ?""
Fin: GRAND SUCCÈS AMOUREUX ♥ - Jules et toi êtes officiellement ensemble ! Score d'amour : {amour}% !
";
        File.WriteAllText(path, contenu, System.Text.Encoding.UTF8);
    }
}
