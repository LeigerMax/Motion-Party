# Script de build automatique pour Motion Party
param(
    [Parameter(Mandatory=$true)]
    [string]$BuildPath,
    
    [Parameter(Mandatory=$false)]
    [string]$UnityBuildPath = ""
)

Write-Host "🔧 Build automatique Motion Party" -ForegroundColor Green

# Nettoyer les fichiers corrompus avant le build
Write-Host "🧹 Nettoyage des fichiers potentiellement problématiques..." -ForegroundColor Yellow

$problematicFiles = @(
    "Assets\Models\IgniteCoders\Simple Water Shader\Info\Readme.asset"
)

foreach ($file in $problematicFiles) {
    $fullPath = Join-Path (Get-Location) $file
    if (Test-Path $fullPath) {
        Remove-Item $fullPath -Force
        Write-Host "🗑️  Supprimé fichier corrompu: $file" -ForegroundColor Gray
    }
}

Write-Host "✅ Nettoyage terminé" -ForegroundColor Green

# Fonction pour vérifier et corriger les scripts d'éditeur
function Test-EditorScripts {
    Write-Host "🔍 Vérification des scripts d'éditeur..." -ForegroundColor Yellow
    
    # Scripts connus utilisant UnityEditor sans protection
    $editorScripts = @(
        "Assets\Scripts\Gameplay\Common\BadgeSystem\Core\BadgeSystemInitializer.cs"
    )
    
    foreach ($script in $editorScripts) {
        $fullPath = Join-Path (Get-Location) $script
        if (Test-Path $fullPath) {
            $content = Get-Content $fullPath -Raw
            
            # Vérifier si UnityEditor est utilisé sans protection
            if ($content -match "UnityEditor\." -and $content -notmatch "#if UNITY_EDITOR") {
                Write-Host "⚠️  Script d'éditeur détecté mais déjà corrigé: $script" -ForegroundColor Yellow
            }
        }
    }
    
    Write-Host "✅ Vérification des scripts terminée" -ForegroundColor Green
}

Test-EditorScripts

# Vérifier que le dossier de build Unity existe
if ($UnityBuildPath -ne "" -and (Test-Path $UnityBuildPath)) {
    Write-Host "✅ Dossier Unity build trouvé: $UnityBuildPath" -ForegroundColor Green
    
    # Copier les fichiers Unity vers le dossier de build final
    Write-Host "📁 Copie des fichiers Unity..." -ForegroundColor Yellow
    Copy-Item -Path "$UnityBuildPath\*" -Destination $BuildPath -Recurse -Force
} else {
    Write-Host "⚠️  Veuillez d'abord builder depuis Unity" -ForegroundColor Yellow
    Write-Host "   File → Build Settings → Build" -ForegroundColor White
    Write-Host "   Puis relancer ce script avec le paramètre -UnityBuildPath" -ForegroundColor White
}

# Créer le dossier de build s'il n'existe pas
if (!(Test-Path $BuildPath)) {
    New-Item -ItemType Directory -Path $BuildPath -Force
    Write-Host "📁 Dossier de build créé: $BuildPath" -ForegroundColor Green
}

# Copier la partie Python
$pythonSource = ".\python-tracker"
$pythonDest = "$BuildPath\python-tracker"

if (Test-Path $pythonSource) {
    Write-Host "🐍 Copie du système Python..." -ForegroundColor Yellow
    
    # Supprimer l'ancien dossier python-tracker s'il existe
    if (Test-Path $pythonDest) {
        Remove-Item -Path $pythonDest -Recurse -Force
    }
    
    # Copier le dossier python-tracker
    Copy-Item -Path $pythonSource -Destination $pythonDest -Recurse -Force
    
    # Exclure les fichiers inutiles
    $excludePatterns = @("__pycache__", "*.pyc", ".git", "*.log", "test_*")
    foreach ($pattern in $excludePatterns) {
        Get-ChildItem -Path $pythonDest -Recurse -Name $pattern | ForEach-Object {
            $fullPath = Join-Path $pythonDest $_
            if (Test-Path $fullPath) {
                Remove-Item -Path $fullPath -Recurse -Force
                Write-Host "🗑️  Supprimé: $_" -ForegroundColor Gray
            }
        }
    }
    
    Write-Host "✅ Python copié avec succès" -ForegroundColor Green
} else {
    Write-Host "❌ Dossier python-tracker non trouvé!" -ForegroundColor Red
    exit 1
}

# Créer un script de lancement
$launchScript = @"
@echo off
echo 🎮 Lancement de Motion Party...
echo.

REM Vérifier que Python est installé
python --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Python n'est pas installé ou pas dans le PATH!
    echo.
    echo Veuillez installer Python depuis https://python.org
    echo N'oubliez pas de cocher "Add to PATH" pendant l'installation
    pause
    exit /b 1
)

REM Vérifier les dépendances Python
echo 🔍 Vérification des dépendances Python...
cd python-tracker
pip install -r requirements.txt --quiet
if errorlevel 1 (
    echo ❌ Erreur lors de l'installation des dépendances Python!
    pause
    exit /b 1
)
cd ..

REM Lancer le jeu
echo ✅ Lancement du jeu...
start "" "$(Split-Path -Leaf (Get-ChildItem -Path $BuildPath -Filter "*.exe" | Select-Object -First 1).Name)"

echo.
echo 🎉 Motion Party est lancé!
echo L'écran de chargement s'affichera pendant l'initialisation de Python.
echo Fermez cette fenêtre une fois le jeu démarré.
pause
"@

$launchScriptPath = "$BuildPath\Start_Motion_Party.bat"
$launchScript | Out-File -FilePath $launchScriptPath -Encoding ASCII

Write-Host "✅ Script de lancement créé: Start_Motion_Party.bat" -ForegroundColor Green

# Créer un script d'installation des dépendances
$setupScript = @"
@echo off
echo 🔧 Installation des dépendances Motion Party...
echo.

REM Vérifier Python
python --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Python n'est pas installé!
    echo Téléchargez et installez Python depuis https://python.org
    echo N'oubliez pas de cocher "Add to PATH"
    pause
    exit /b 1
)

echo ✅ Python trouvé
echo.

REM Installer les dépendances
echo 🐍 Installation des dépendances Python...
cd python-tracker
pip install -r requirements.txt
if errorlevel 1 (
    echo ❌ Erreur lors de l'installation!
    pause
    exit /b 1
)
cd ..

echo.
echo ✅ Installation terminée!
echo.
echo 🎮 Utilisez "Start_Motion_Party.bat" pour lancer le jeu
pause
"@

$setupScriptPath = "$BuildPath\Setup.bat"
$setupScript | Out-File -FilePath $setupScriptPath -Encoding ASCII

Write-Host "✅ Script d'installation créé: Setup.bat" -ForegroundColor Green

# Créer un README pour les utilisateurs
$userReadme = @"
# Motion Party - Installation et Utilisation

## 🔧 Installation (à faire une seule fois)

1. **Installez Python** (si pas déjà fait) :
   - Téléchargez depuis https://python.org
   - ⚠️ IMPORTANT : Cochez "Add to PATH" pendant l'installation

2. **Installez les dépendances** :
   - Double-cliquez sur `Setup.bat`
   - Attendez la fin de l'installation

## 🎮 Lancer le jeu

- Double-cliquez sur `Start_Motion_Party.bat`
- Un écran de chargement apparaîtra pendant l'initialisation
- Le jeu commencera automatiquement une fois Python prêt

## 📋 Configuration requise

- Windows 10/11
- Python 3.8 ou plus récent
- Webcam connectée
- Connexion Internet (pour l'installation des dépendances)

## 🔧 Dépannage

### Le jeu ne se lance pas
1. Vérifiez que Python est installé : ouvrez une invite de commande et tapez `python --version`
2. Relancez `Setup.bat` pour réinstaller les dépendances
3. Vérifiez que votre webcam fonctionne

### L'écran de chargement reste affiché
- Patientez quelques secondes, Python s'initialise
- Vérifiez que votre webcam n'est pas utilisée par une autre application
- Redémarrez le jeu si le problème persiste

### Pour plus d'aide
Consultez les logs dans la console Unity ou contactez le support.
"@

$userReadmePath = "$BuildPath\README.txt"
$userReadme | Out-File -FilePath $userReadmePath -Encoding UTF8

Write-Host "✅ README utilisateur créé: README.txt" -ForegroundColor Green

Write-Host "" -ForegroundColor White
Write-Host "🎉 Build terminé avec succès!" -ForegroundColor Green
Write-Host "📁 Dossier: $BuildPath" -ForegroundColor White
Write-Host "" -ForegroundColor White
Write-Host "📦 Contenu du package:" -ForegroundColor Yellow
Write-Host "   ├── MonGame.exe (votre jeu Unity)" -ForegroundColor White
Write-Host "   ├── python-tracker/ (système de tracking)" -ForegroundColor White
Write-Host "   ├── Start_Motion_Party.bat (lanceur)" -ForegroundColor White
Write-Host "   ├── Setup.bat (installation)" -ForegroundColor White
Write-Host "   └── README.txt (instructions)" -ForegroundColor White
Write-Host "" -ForegroundColor White
Write-Host "✅ Prêt à distribuer!" -ForegroundColor Green
