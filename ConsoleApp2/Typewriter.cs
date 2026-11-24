namespace ConsoleApp2;

public static class Typewriter
{
    /// <summary>
    /// Vitesse de défilement par défaut (millisecondes par caractère).
    /// ~25ms correspond à la vitesse des textes rétro (type Pokémon).
    /// </summary>
    public static int DefaultSpeedMs { get; set; } = 25;

    /// <summary>
    /// Affiche du texte lettre par lettre comme dans les anciens jeux Pokémon.
    /// Si le joueur appuie sur une touche, le reste de la phrase s'affiche instantanément.
    /// </summary>
    public static void Write(string text, int delayMs = -1, bool allowSkip = true)
    {
        if (string.IsNullOrEmpty(text)) return;

        if (delayMs < 0) delayMs = DefaultSpeedMs;

        // Si l'entrée est redirigée (tests / scripts) ou délai à 0, affichage direct
        if (Console.IsInputRedirected || delayMs == 0)
        {
            Console.Write(text);
            return;
        }

        bool skip = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            Console.Write(c);

            // Permettre au joueur de passer le défilement en appuyant sur une touche
            if (allowSkip && !skip && Console.KeyAvailable)
            {
                Console.ReadKey(intercept: true);
                skip = true;
            }

            if (!skip)
            {
                int pause = delayMs;

                // Pauses expressives sur la ponctuation (style Pokémon)
                if (c is '.' or '!' or '?' or ':')
                {
                    pause = Math.Max(delayMs * 5, 120);
                }
                else if (c is ',' or ';' or '…')
                {
                    pause = Math.Max(delayMs * 3, 70);
                }
                else if (c == '\n')
                {
                    pause = Math.Max(delayMs * 2, 50);
                }

                Thread.Sleep(pause);
            }
        }
    }

    /// <summary>
    /// Affiche du texte lettre par lettre suivi d'un saut de ligne.
    /// </summary>
    public static void WriteLine(string text = "", int delayMs = -1, bool allowSkip = true)
    {
        Write(text + Environment.NewLine, delayMs, allowSkip);
    }

    /// <summary>
    /// Vide le tampon du clavier pour éviter que les touches pressées pour passer le texte
    /// ne polluent la prochaine question posée à l'utilisateur.
    /// </summary>
    public static void FlushInput()
    {
        if (!Console.IsInputRedirected)
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(intercept: true);
            }
        }
    }
}
