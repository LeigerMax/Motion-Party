@echo off
echo =================================
echo    Motion Party Setup
echo =================================
echo.

REM Vérifier si Python est installé
echo [1/4] Vérification de Python...
python --version >nul 2>&1
if errorlevel 1 (
    echo ERREUR: Python n'est pas installé.
    echo.
    echo Veuillez installer Python depuis https://www.python.org/
    echo Assurez-vous de cocher "Add Python to PATH" durant l'installation.
    echo.
    pause
    exit /b 1
) else (
    echo Python détecté: 
    python --version
)

echo.
echo [2/4] Vérification des dossiers...
if not exist "python-tracker" (
    echo ERREUR: Le dossier python-tracker n'a pas été trouvé.
    pause
    exit /b 1
)

if not exist "python-tracker\requirements.txt" (
    echo ATTENTION: requirements.txt non trouvé.
) else (
    echo requirements.txt trouvé.
)

echo.
echo [3/4] Installation des dépendances Python...
cd python-tracker

echo Installation de pip si nécessaire...
python -m ensurepip --upgrade >nul 2>&1

echo Mise à jour de pip...
python -m pip install --upgrade pip

echo Installation des packages requis...
python -m pip install -r requirements.txt

if errorlevel 1 (
    echo.
    echo ERREUR lors de l'installation des dépendances.
    echo Vérifiez votre connexion internet et réessayez.
    cd ..
    pause
    exit /b 1
)

cd ..

echo.
echo [4/4] Test de configuration...
echo Test de la communication UDP...
python -c "import socket; s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM); s.bind(('127.0.0.1', 5052)); s.close(); print('Port UDP 5052 disponible')" 2>nul
if errorlevel 1 (
    echo ATTENTION: Le port UDP 5052 pourrait être occupé.
)

echo.
echo =================================
echo    Setup terminé avec succès!
echo =================================
echo.
echo Vous pouvez maintenant:
echo 1. Lancer Motion Party via Unity Editor
echo 2. Ou utiliser start_motion_party.bat pour la version build
echo.
echo Pour tester Python séparément:
echo   cd python-tracker
echo   python main.py
echo.
pause
