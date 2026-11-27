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

### 👤 Personnalisation du Joueur & Easter Eggs de l'Âge
- Choix du **Nom**, de **l'Âge** et du **Sexe** (*Homme*, *Femme*, *Non-binaire*).
- **Easter Eggs Humoristiques sur l'Âge** :
  - *Moins de 18 ans* :
    - 1ère tentative : `Je vais appeler les flics , le beau Jules ne fréquente pas les mineurs` (refusé).
    - 2ème tentative : `Oh après tout l'age n'est qu'un concept moderne établie par des gens trop peu ouvert d'esprit` (validé !).
  - *Plus de 65 ans* :
    - 1ère tentative : `Désolé mais le beau Jules ne mange pas de pomme Granny` (refusé).
    - 2ème tentative : `Bon allé pourquoi pas se laisser une petite chance, on peut se trouver un point commun` (validé !).

### 🗺️ Multi-Dates & Enchaînement des Lieux
- Le jeu gère une suite de plusieurs rendez-vous dans des fichiers distincts :
  1. **Le Date au Parc** (`situation_parc.txt`) : La rencontre fortuite, le café renversé, le stand de glaces.
  2. **Une Journée à l'École** (`situation_ecole.txt`) : Retrouvailles devant les casiers, TP de sciences en binôme, confession sous les cerisiers.
  3. **Soirée Intime chez Jules** (`situation_maison.txt`) : L'ambiance feutrée, le verre sur le canapé, la tension intime... jusqu'au grand coup de filet de la **Brigade de Contrôle des Enseignants** !
- Transition automatique et fluide d'un lieu au suivant dès la fin d'un rendez-vous.

### 🚨 Le Grand Coup de Théâtre & Crédits Officiels
- À l'apogée du date intime chez Jules, le joueur révèle sa véritable identité : **Capitaine sous couverture de la Brigade de Contrôle des Enseignants**.
- Assaut des forces d'intervention, arrestation de Jules en caleçon et générique de fin aux couleurs de la **Police Nationale** !

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
