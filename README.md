# ♥ HowToDateJules ♥
> *Le simulateur de séduction officiel en console !*

Bienvenue dans **How To Date Jules**, un visual novel / jeu de drague textuel en console développé en C# (.NET). Vous y incarnez un personnage qui tente de conquérir le cœur de Jules à travers différents rendez-vous et situations interactives. Vos choix influenceront directement la jauge d'amour de Jules et détermineront l'issue de votre histoire d'amour !

---

## 📑 Sommaire
- [✨ Fonctionnalités Principales](#-fonctionnalités-principales)
- [💖 Système de Jauge d'Amour](#-système-de-jauge-damour)
- [💾 Système de Sauvegarde & Reprise Continue](#-système-de-sauvegarde--reprise-continue)
- [📜 Moteur de Scénario Textuel](#-moteur-de-scénario-textuel)
- [📂 Structure du Projet](#-structure-du-projet)
- [🚀 Installation & Lancement](#-installation--lancement)
- [✍️ Comment Créer de Nouveaux Scénarios](#️-comment-créer-de-nouveaux-scénarios)

---

## ✨ Fonctionnalités Principales

### 🎮 Défilement Rétro façon Pokémon (`Typewriter`)
- **Effet machine à écrire** lettre par lettre (~25 ms/caractère) pour une immersion rétro digne des RPG Game Boy.
- **Pauses expressives** sur la ponctuation (virgules, points, points d'exclamation/interrogation).
- **Passage rapide (*Skip*)** : Appuyez sur n'importe quelle touche (Espace, Entrée...) pour afficher immédiatement la suite du texte.
- **Tampon sécurisé (`FlushInput`)** : Les touches pressées pour passer le texte ne polluent pas vos saisies de choix.

### 👤 Personnalisation du Joueur & Easter Egg de l'Âge
- Choix du **Nom**, de **l'Âge** et du **Sexe** (*Homme*, *Femme*, *Non-binaire*).
- **Easter Egg Humoristique sur l'Âge** :
  - *1ère saisie d'un âge < 18 ans* : `Je vais appeler les flics , le beau Jules ne fréquente pas les mineurs` (saisie refusée).
  - *2ème saisie d'un âge < 18 ans* : `Oh après tout l'age n'est qu'un concept moderne établie par des gens trop peu ouvert d'esprit` (âge validé !).

### 🗺️ Multi-Dates & Enchaînement des Lieux
- Le jeu gère une suite de plusieurs rendez-vous dans des fichiers distincts :
  1. **Le Date au Parc** (`situation_parc.txt`) : La rencontre fortuite, le café renversé, le stand de glaces.
  2. **Une Journée à l'École** (`situation_ecole.txt`) : Retrouvailles devant les casiers, TP de sciences en binôme, confession sous les cerisiers.
- Transition automatique et fluide d'un lieu au suivant dès la fin d'un rendez-vous.

### 📊 Tableau Récapitulatif de Fin de Date
À la fin de chaque lieu, un bilan complet s'affiche :
- `[✓ BON CHOIX]` : Choix ayant augmenté l'amour de Jules (`+X% ♥`).
- `[✗ MAUVAIS CHOIX]` : Choix ayant refroidi Jules (`-X% 💔`).
- `[~ NEUTRE]` : Choix sans impact.
- **Bilan chiffré du date** : Évolution nette de l'affection (`+35% d'amour`) et commentaire personnalisé de Jules selon vos performances.

---

## 💖 Système de Jauge d'Amour

Jules possède une jauge d'affection qui démarre à **20%** (*Intrigué*) et varie entre **0%** et **100%** selon vos répliques.

```text
Jauge d'Amour : [ ♥♥♥♥♥♥♡♡♡♡ ] 55% (😊 Complice)
```

| Paliers d'Affection | Statut de la Relation | Réaction de Jules |
| :--- | :--- | :--- |
| **0% à 19%** | ❄️ *Froid & Distant* | Jules est distant et peut s'enfuir (Game Over). |
| **20% à 39%** | 💭 *Intrigué* | Curieux d'en savoir plus sur vous. |
| **40% à 59%** | 😊 *Complice* | La conversation coule de source, les sourires s'installent. |
| **60% à 79%** | 💖 *Coup de cœur* | Jules commence sérieusement à craquer pour vous. |
| **80% à 100%** | 💘 *Fou amoureux !* | Jules est prêt à vous déclarer sa flamme éternelle ! |

---

## 💾 Système de Sauvegarde & Reprise Continue

Le jeu intègre une persistance automatique dans le fichier **`sauvegarde.txt`** :

```txt
=== SAUVEGARDE HOW TO DATE JULES ===
Nom: Camille
Age: 21
Sexe: Femme
Affection: 55
Scenario: situation_ecole.txt
SituationId: 2
TitreLieu: Une Journée à l'École
JeuTermine: False
Date: 2026-11-24 12:41:31
```

* **Reprise exacte de la partie** : Si vous fermez le jeu après la 1ère scène ou au milieu d'un date, le jeu reprendra **précisément à cette même scène** au redémarrage !
* **Détection automatique au lancement** : Propose de reprendre la partie existante ou de créer un nouveau personnage.
* **Fin de jeu** : Si l'aventure est terminée (`JeuTermine: True`), le jeu propose de revivre l'histoire depuis le début ou de créer une nouvelle partie.

---

## 📜 Moteur de Scénario Textuel

Les histoires sont écrites dans de simples fichiers texte `.txt`. Le moteur interprète les balises, applique les embranchements et substitue dynamiquement les variables :

* `{nom}` / `{joueur}` : Le nom du joueur.
* `{age}` : L'âge du joueur.
* `{sexe}` : Le sexe du joueur.
* `{amour}` / `{affection}` : Le score actuel d'amour avec Jules.

---

## 📂 Structure du Projet

```text
HowToDateJules/
│
├── ConsoleApp2/                      # Projet principal C# (.NET 11)
│   ├── Program.cs                    # Point d'entrée, création de perso & menu principal
│   ├── Player.cs                     # Modèle de données du joueur & progression
│   ├── SaveManager.cs                # Gestion de la lecture/écriture dans sauvegarde.txt
│   ├── ScenarioManager.cs            # Moteur de jeu (parsing, affichage, récap, dates)
│   ├── Situation.cs                  # Modèles de scènes, choix et historique
│   ├── Typewriter.cs                 # Moteur d'affichage texte rétro style Pokémon
│   │
│   ├── situation_parc.txt            # Scénario du 1er Date : Le Parc
│   ├── situation_ecole.txt           # Scénario du 2nd Date : L'École
│   └── situation.txt                 # Scénario par défaut (synchronisé avec le Parc)
│
├── README.md                         # Documentation complète du jeu
└── ConsoleApp2.slnx                  # Fichier de solution
```

---

## 🚀 Installation & Lancement

### Prérequis
- [.NET SDK](https://dotnet.microsoft.com/download) (version 8.0, 9.0 ou 11.0 installée sur votre machine).

### Lancer le jeu
Ouvrez un terminal dans le dossier du projet et tapez :

```bash
dotnet run --project ConsoleApp2
```

Ou ouvrez la solution dans votre IDE préféré (**Visual Studio**, **JetBrains Rider**, ou **VS Code**) et lancez le projet `ConsoleApp2`.

---

## ✍️ Comment Créer de Nouveaux Scénarios

Pour créer un nouveau date (par exemple `situation_cinema.txt`), créez simplement un fichier texte avec la structure suivante :

```txt
TITRE_DATE: Soirée au Cinéma
SUIVANT: situation_restaurant.txt

=== SITUATION 1 ===
Titre: Le choix du film
Texte:
Tu retrouves Jules devant les affiches lumineuses du cinéma.
"Salut {nom} ! Quel genre de film te tente ce soir ?"
Choix:
1 | "Un film romantique... pour être dans l'ambiance !" -> 2 | +20
2 | "Un film d'horreur, comme ça tu pourras me protéger." -> 2 | +15
3 | "Peu importe, tant qu'il y a du popcorn." -> 3 | -10

=== SITUATION 2 ===
Titre: La salle obscure
Texte:
Vous vous installez côte à côte au dernier rang...
...
Fin: Fin d'une merveilleuse séance de cinéma !
Suivant: situation_restaurant.txt
```

### Règles de syntaxe des choix :
- `Numéro | Texte du choix -> ID_CIBLE | +VALEUR` *(Bonus d'amour)*
- `Numéro | Texte du choix -> ID_CIBLE | -VALEUR` *(Malus d'amour)*
- `Numéro | Texte du choix -> ID_CIBLE` *(Choix neutre)*

---

*Développé avec amour pour séduire Jules ♥*
