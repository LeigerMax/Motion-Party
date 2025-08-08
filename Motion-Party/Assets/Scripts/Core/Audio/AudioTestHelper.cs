using UnityEngine;
using Core.Audio;

namespace Core.Audio
{
    /// <summary>
    /// Outil simple pour tester les effets sonores de l'AudioManager
    /// Utile pendant le développement pour vérifier que les sons fonctionnent
    /// </summary>
    public class AudioTestHelper : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField] private bool enableKeyboardTesting = true;
        
        [Header("Debug")]
        [SerializeField] private bool showInstructions = true;

        void Update()
        {
            if (!enableKeyboardTesting) return;

            // Test du son d'eau avec la touche W
            if (Input.GetKeyDown(KeyCode.W))
            {
                TestWaterSplash();
            }

            // Test du son de capture de libellule avec la touche F
            if (Input.GetKeyDown(KeyCode.F))
            {
                TestFireflyCapture();
            }

            // Test du rechargement de config avec la touche R
            if (Input.GetKeyDown(KeyCode.R))
            {
                TestReloadConfig();
            }
        }

        void OnGUI()
        {
            if (!showInstructions) return;

            // Afficher les instructions de test en haut à gauche
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 14;
            style.alignment = TextAnchor.UpperLeft;

            string instructions = "TESTS AUDIO:\n" +
                                "W = Water Splash 💧\n" +
                "F = Firefly Capture 🦋\n" +
                                "R = Reload Config 🔄";

            GUI.Box(new Rect(10, 10, 200, 100), instructions, style);
        }

        /// <summary>
        /// Teste le son d'éclaboussure d'eau
        /// </summary>
        public void TestWaterSplash()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWaterSplash();
                Debug.Log("🧪 Test: Water Splash joué");
            }
            else
            {
                Debug.LogWarning("⚠️ AudioManager non disponible pour le test");
            }
        }

        /// <summary>
        /// Teste le son de capture de libellule
        /// </summary>
        public void TestFireflyCapture()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayFireflyCapture();
                Debug.Log("🧪 Test: Firefly Capture joué");
            }
            else
            {
                Debug.LogWarning("⚠️ AudioManager non disponible pour le test");
            }
        }

        /// <summary>
        /// Teste le rechargement de la configuration audio
        /// </summary>
        public void TestReloadConfig()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ReloadConfiguration();
                Debug.Log("🧪 Test: Configuration audio rechargée");
            }
            else
            {
                Debug.LogWarning("⚠️ AudioManager non disponible pour le test de rechargement");
            }
        }
    }
}
