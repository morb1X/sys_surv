\# System Survivor

\*\*Ein Top-Down Survival Shooter im Cyberpunk-/Sci-Fi-Stil, inspiriert von Vampire Survivors.\*\*

\## 🎮 Über das Spiel  
System Survivor ist ein Casual-Game mit Bullet-Hell-Elementen. Das Ziel ist es, Wellen von tausenden Gegnern bis zur 30-Minuten-Marke zu überleben\[cite: 9]. Der Spieler steuert lediglich die Bewegung, während die Waffen automatisch feuern ("One-Handed Play")\[cite: 9, 10]. Das Spielgefühl entwickelt sich von "verletzlich und überwältigt" hin zu einer "unaufhaltsamen Gottheit" mit extremer Skalierung\[cite: 9, 10].

\## 🛠️ Technologie  
\*   \*\*Sprache:\*\* C#\[cite: 10]  
\*   \*\*Framework:\*\* Windows Forms / .NET 10\[cite: 10]

\## ✨ Features  
\*   \*\*Prozedurale Kartengenerierung:\*\* Cellular Automata (45% Zufallsfüllung, Glättung)\[cite: 10].  
\*   \*\*Intelligente Gegner-KI:\*\* BFS Heatmap Algorithmus zur Wegfindung\[cite: 10].  
\*   \*\*AABB Kollisionserkennung:\*\* 4-Ecken Prüfung, getrennte X/Y-Auswertung\[cite: 10].  
\*   \*\*Progression \& Upgrades:\*\* XP-System (geometrische Skalierung), 6 Upgrade-Pfade (Speed, HP, Piercing etc.)\[cite: 10].  
\*   \*\*Benutzerdefiniertes UI:\*\* Hauptmenü (mit Conway's Game of Life), Pause-Menü und Developer Console\[cite: 10].

\## 🚀 Status des Projekts \& Lernziele  
Dieses Projekt wurde entwickelt, um Spiellogik, Rendering und Algorithmen in C# zu verstehen. Da es sich um ein frühes Lernprojekt handelt, liegt der Fokus aktuell auf der Funktionalität und noch nicht auf einer perfekten Softwarearchitektur.

\*Bereiche für zukünftige Verbesserungen (To-Do):\*  
\*   Refactoring des Codes (Clean Code Struktur).  
\*   Trennung von Spiellogik und Benutzeroberfläche (z.B. MVVM Muster).  
\*   Umstieg von Windows Forms auf eine dedizierte Game Engine oder WPF für bessere Performance.

