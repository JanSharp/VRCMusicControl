using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    /// <summary>
    /// <para>This script does not have a public api at this time.</para>
    /// </summary>
    public class BasicMusicControlTimerUI : UdonSharpBehaviour
    {
        [Tooltip("Must not be null.")]
        [SerializeField] private BasicMusicControlTimer sharedTimer;
        [Tooltip("The time frame is the timer working with. It starts at minTime, and goes until maxTime and "
            + "loops back around.\n"
            + "Used to properly scale the time slider. The input field will move the input value into this "
            + "range using modulo.")]
        [SerializeField] private float minTime = 0f;
        [Tooltip("The time frame is the timer working with. It starts at minTime, and goes until maxTime and "
            + "loops back around.\n"
            + "Used to properly scale the time slider. The input field will move the input value into this "
            + "range using modulo.")]
        [SerializeField] [Min(1f)] private float maxTime = 1f;
        [Tooltip("The min speed that can be set using the speed slider. "
            + "The input field allows exceeding this limitation.")]
        [SerializeField] private float minSliderSpeed = 1f;
        [Tooltip("The max speed that can be set using the speed slider. "
            + "The input field allows exceeding this limitation.")]
        [SerializeField] private float maxSliderSpeed = 60f;
        [Tooltip("How many times the UI should update while it is not hidden. Since Udon is slow, UI "
            + "updates can be expensive when done every frame, however keep in mind that the 2 settings "
            + "below cause the UI to be hidden 99% of the time when it is not used, therefore some more "
            + "frequent updates are acceptable.")]
        [SerializeField] [Min(0f)] private float uiUpdatesPerSecond = 10f;
        [Tooltip("When true, the UI is hidden from the start of the world and the player must click on it in "
            + "order for it to start updating. This saves performance for everyone who never needs to touch "
            + "nor see the UI ever, because UI updates are expensive because Udon is slow.")]
        [SerializeField] private bool hiddenByDefault = true;
        [Tooltip("If the player is more than this distance away from the UI position in world space, the UI"
            + "will automatically hide itself, which pauses UI updates and therefore saves performance. "
            + "Useful for those who do touch and see the UI, while still remaining performant.")]
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
        private float totalTime;
        private bool isHidden;
        private bool updateLoopRunning;
        private int lastAutoHideCheck = -1;
        private const float AutoHideCheckInterval = 10f;

        private void Start()
        {
            totalTime = maxTime - minTime;
            timeSlider.minValue = minTime;
            timeSlider.maxValue = maxTime;
            speedSlider.minValue = minSliderSpeed;
            speedSlider.maxValue = maxSliderSpeed;
            isHidden = hiddenByDefault;
            sharedTimer.RegisterOnTimerReady(this);
            sharedTimer.RegisterOnTimerSettingsChanged(this);
        }

        public void OnTimerReady()
        {
            loadingOverlay.SetActive(false);
            CheckHiddenState(); // This also starts the update loop, if it should be started.
        }

        private void StartUpdateLoop()
        {
            if (updateLoopRunning)
                return;
            updateLoopRunning = true;
            SendCustomEventDelayedSeconds(nameof(InternalUpdateLoop), 1f / uiUpdatesPerSecond);
        }

        public void OnTimerSettingsChanged()
        {
            if (isHidden)
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
            float time = (sharedTimer.CurrentTime - minTime) % totalTime;
            if (time < 0f)
                time += totalTime;
            time += minTime;
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
            if (isHidden)
            {
                updateLoopRunning = false;
                return;
            }
            UpdateTimeUI();
            SendCustomEventDelayedSeconds(nameof(InternalUpdateLoop), 1f / uiUpdatesPerSecond);
        }

        private bool IsBeyondHideRange()
            => Vector3.Distance(Networking.LocalPlayer.GetPosition(), transform.position) >= autoHideDistance;

        private void CheckHiddenState()
        {
            if (!isHidden)
                isHidden = IsBeyondHideRange();
            OnTimerSettingsChanged(); // To update settings when its no longer hidden.
            hiddenOverlay.SetActive(isHidden);
            if (!isHidden)
                StartUpdateLoop();
        }

        public void OnTimeSliderValueChanged() => sharedTimer.CurrentTime = timeSlider.value;
        public void OnTimeTextFieldConfirmed() => sharedTimer.CurrentTime = float.Parse(timeInput.text);
        public void OnSpeedSliderValueChanged() => sharedTimer.Speed = speedSlider.value;
        public void OnSpeedTextFieldConfirmed() => sharedTimer.Speed = float.Parse(speedInput.text);
        public void OnPauseButtonClick() => sharedTimer.IsPaused = !sharedTimer.IsPaused;

        public void OnHideButtonClick()
        {
            isHidden = true;
            CheckHiddenState();
        }

        public void OnShowButtonClick()
        {
            isHidden = false;
            CheckHiddenState();
        }
    }
}
