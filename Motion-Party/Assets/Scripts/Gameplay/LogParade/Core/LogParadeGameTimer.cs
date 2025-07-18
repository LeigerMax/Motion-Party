using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Gameplay.LogParade.Utils;

namespace Gameplay.LogParade.Core
{
    /// <summary>
    /// Gère le timer du jeu LogParade
    /// </summary>
    public class LogParadeGameTimer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float gameDuration = 60f;
        [SerializeField] private bool startOnAwake = false;
        [SerializeField] private bool countDown = true;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Events")]
        public UnityEvent OnTimeUp;

        private float currentTime;
        private bool isRunning = false;

        private void Start()
        {
            if (startOnAwake)
            {
                StartTimer();
            }
        }

        private void Update()
        {
            if (!isRunning) return;

            if (countDown)
            {
                currentTime -= Time.deltaTime;
                if (currentTime <= 0)
                {
                    currentTime = 0;
                    StopTimer();
                    OnTimeUp?.Invoke();
                }
            }
            else
            {
                currentTime += Time.deltaTime;
                if (currentTime >= gameDuration)
                {
                    currentTime = gameDuration;
                    StopTimer();
                    OnTimeUp?.Invoke();
                }
            }

            UpdateTimerDisplay();
        }

        public void StartTimer()
        {
            currentTime = countDown ? gameDuration : 0f;
            isRunning = true;
            UpdateTimerDisplay();
        }

        public void StopTimer()
        {
            isRunning = false;
            UpdateTimerDisplay();
        }

        public void PauseTimer()
        {
            isRunning = false;
        }

        public void ResumeTimer()
        {
            isRunning = true;
        }

        public void SetDuration(float duration)
        {
            gameDuration = duration;
            if (!isRunning)
            {
                currentTime = countDown ? duration : 0f;
                UpdateTimerDisplay();
            }
        }

        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                float timeToDisplay = countDown ? currentTime : (gameDuration - currentTime);
                int minutes = Mathf.FloorToInt(timeToDisplay / 60);
                int seconds = Mathf.FloorToInt(timeToDisplay % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }

        public float GetRemainingTime()
        {
            return countDown ? currentTime : (gameDuration - currentTime);
        }

        public float GetElapsedTime()
        {
            return countDown ? (gameDuration - currentTime) : currentTime;
        }

        public bool IsRunning()
        {
            return isRunning;
        }
    }
}
