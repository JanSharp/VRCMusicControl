using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BasicMusicControlTimerUI : UdonSharpBehaviour
    {
        [SerializeField] private BasicMusicControlTimer sharedTimer;
        [SerializeField] [Min(1f)] private float maxTime = 1f;
        [SerializeField] private float minSliderSpeed = 1f;
        [SerializeField] private float maxSliderSpeed = 60f;
        [SerializeField] [Min(0f)] private float uiUpdatesPerSecond = 10f;
        [SerializeField] private bool hiddenByDefault = true;
        [SerializeField] [Min(0f)] private float autoHideDistance = 16f;
        [Header("Internal")]
        [SerializeField] private Slider timeSlider;
        [SerializeField] private Slider speedSlider;
        [SerializeField] private InputField timeInput;
        [SerializeField] private InputField speedInput;
        [SerializeField] private TextMeshProUGUI pauseButtonText;
        [SerializeField] private Toggle autoHideToggle;
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private GameObject hiddenOverlay;
        private bool isManuallyHidden;
        private bool isOutOfRangeHidden;
        private bool IsHidden => isManuallyHidden || isOutOfRangeHidden;
        private bool updateLoopRunning;
        private int lastAutoHideCheck = -1;
        private const float AutoHideCheckInterval = 10f;

        private void Start()
        {
            timeSlider.maxValue = maxTime;
            speedSlider.minValue = minSliderSpeed;
            speedSlider.maxValue = maxSliderSpeed;
            isManuallyHidden = hiddenByDefault;
            sharedTimer.RegisterOnTimerReady(this);
            sharedTimer.RegisterOnTimerSettingsChanged(this);
        }

        public void OnTimerReady()
        {
            Debug.Log($"OnTimerReady {sharedTimer.IsReady} {sharedTimer.CurrentTime} {sharedTimer.Speed} {sharedTimer.IsPaused}");
            loadingOverlay.SetActive(false);
            CheckHiddenState();
            StartUpdateLoop();
        }

        private void StartUpdateLoop()
        {
            updateLoopRunning = true;
            SendCustomEventDelayedSeconds(nameof(InternalUpdateLoop), 1f / uiUpdatesPerSecond);
        }

        public void OnTimerSettingsChanged()
        {
            if (IsHidden)
                return;

            float speed = sharedTimer.Speed;
            speedSlider.SetValueWithoutNotify(speed);
            // SetTextWithoutNotify is not exposed, settings text like this does not raise the end text event
            // which means it's fine for this use case. My guess is that it does raise the value changed event.
            speedInput.text = speed.ToString("f3");

            pauseButtonText.text = sharedTimer.IsPaused ? "Paused" : "Running";

            UpdateTimeUI();
        }

        private void UpdateTimeUI()
        {
            float time = sharedTimer.CurrentTime % maxTime;
            if (time < 0f)
                time += maxTime;
            timeSlider.SetValueWithoutNotify(time);
            if (!timeInput.isFocused)
                timeInput.text = time.ToString("f3");
        }

        public void InternalUpdateLoop()
        {
            if (autoHideToggle.isOn)
            {
                int counter = (int)(Time.time / AutoHideCheckInterval);
                if (lastAutoHideCheck != counter)
                {
                    lastAutoHideCheck = counter;
                    CheckHiddenState();
                }
            }
            if (IsHidden)
            {
                updateLoopRunning = false;
                return;
            }
            UpdateTimeUI();
            SendCustomEventDelayedSeconds(nameof(InternalUpdateLoop), 1f / uiUpdatesPerSecond);
        }

        private void CalculateOutOfRangeHidden()
        {
            isOutOfRangeHidden
                = Vector3.Distance(Networking.LocalPlayer.GetPosition(), transform.position) >= autoHideDistance;
        }

        private void CheckHiddenState()
        {
            if (!isManuallyHidden)
                CalculateOutOfRangeHidden();
            OnTimerSettingsChanged(); // To update settings when its no longer hidden.
            hiddenOverlay.SetActive(IsHidden);
            if (!IsHidden && !updateLoopRunning)
                StartUpdateLoop();
        }

        public void OnTimeSliderValueChanged()
        {
            sharedTimer.CurrentTime = timeSlider.value;
        }

        public void OnTimeTextFieldConfirmed()
        {
            sharedTimer.CurrentTime = float.Parse(timeInput.text);
        }

        public void OnSpeedSliderValueChanged()
        {
            sharedTimer.Speed = speedSlider.value;
        }

        public void OnSpeedTextFieldConfirmed()
        {
            sharedTimer.Speed = float.Parse(speedInput.text);
        }

        public void OnPauseButtonClick()
        {
            sharedTimer.IsPaused = !sharedTimer.IsPaused;
        }

        public void OnHideButtonClick()
        {
            isManuallyHidden = true;
            CheckHiddenState();
        }

        public void OnShowButtonClick()
        {
            isManuallyHidden = false;
            CheckHiddenState();
        }
    }
}
