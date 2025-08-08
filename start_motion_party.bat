@echo off
echo Starting Motion Party...

REM Vérifier si Python est installé
python --version >nul 2>&1
if errorlevel 1 (
    echo ERREUR: Python n'est pas installé ou n'est pas dans le PATH.
    echo Veuillez installer Python depuis https://www.python.org/
    pause
    exit /b 1
)

REM Vérifier si le dossier python-tracker existe
if not exist "python-tracker" (
    echo ERREUR: Le dossier python-tracker n'a pas été trouvé.
    echo Assurez-vous que ce fichier batch est dans le même dossier que python-tracker.
    pause
    exit /b 1
)

REM Installer les dépendances Python si requirements.txt existe
if exist "python-tracker\requirements.txt" (
    echo Installation des dépendances Python...
    cd python-tracker
    python -m pip install -r requirements.txt
    cd ..
)

REM Démarrer le jeu Unity
echo Démarrage de Motion Party...
start "" "Motion Party.exe"

REM Attendre un peu que Unity se lance
timeout /t 3 /nobreak >nul

REM Démarrer le tracker Python
echo Démarrage du tracker Python...
cd python-tracker
python main.py

echo Motion Party fermé.
pause
