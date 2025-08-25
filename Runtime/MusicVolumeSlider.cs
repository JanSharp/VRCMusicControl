using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MusicVolumeSlider : UdonSharpBehaviour
    {
        [SerializeField] private MusicManager manager;
        [SerializeField] private Slider volumeSlider;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public MusicManager Manager => manager;
        public Slider VolumeSlider => volumeSlider;
#endif

        public void OnSliderValueChanged()
        {
            float minValue = volumeSlider.minValue;
            float maxValue = volumeSlider.maxValue;
            float normalizedValue = (volumeSlider.value - minValue) / (maxValue - minValue);
            manager.Volume = normalizedValue;
        }
    }
}
