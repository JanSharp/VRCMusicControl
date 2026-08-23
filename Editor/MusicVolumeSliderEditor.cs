using System.Collections.Generic;
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace JanSharp
{
    public static class MusicVolumeSliderOnBuild
    {
        [OrderedInitializeOnLoad]
        private static void OnAssemblyLoad() => OnBuildUtil.RegisterType<MusicVolumeSlider>(OnBuild);

        private static bool OnBuild(MusicVolumeSlider musicVolumeSlider)
        {
            bool invalid = false;
            if (musicVolumeSlider.VolumeSlider == null)
            {
                Debug.LogError($"[MusicControl] Invalid {nameof(MusicVolumeSlider)}, "
                    + "missing Volume Slider reference.", musicVolumeSlider);
                invalid = true;
            }
            if (musicVolumeSlider.Manager == null)
            {
                Debug.LogError($"[MusicControl] Invalid {nameof(MusicVolumeSlider)}, "
                    + "missing Manager reference.", musicVolumeSlider);
                invalid = true;
            }
            if (invalid)
                return false;

            SerializedObject sliderSo = new SerializedObject(musicVolumeSlider.VolumeSlider);
            // Decided to leave these untouched.
            // sliderSo.FindProperty("m_MinValue").floatValue = 0f;
            // sliderSo.FindProperty("m_MaxValue").floatValue = 1f;
            float minValue = musicVolumeSlider.VolumeSlider.minValue;
            float maxValue = musicVolumeSlider.VolumeSlider.maxValue;
            sliderSo.FindProperty("m_Value").floatValue = minValue + musicVolumeSlider.Manager.Volume * (maxValue - minValue);
            EditorUtil.EnsureHasPersistentSendCustomEventListener(
                sliderSo.FindProperty("m_OnValueChanged"),
                UdonSharpEditorUtility.GetBackingUdonBehaviour(musicVolumeSlider),
                nameof(MusicVolumeSlider.OnSliderValueChanged));
            sliderSo.ApplyModifiedProperties();

            return true;
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(MusicVolumeSlider))]
    public class MusicVolumeSliderEditor : Editor
    {
        private static void SetSliderToThis(IEnumerable<MusicVolumeSlider> volumeSliders)
        {
            foreach (var volumeSlider in volumeSliders)
            {
                SerializedObject so = new SerializedObject(volumeSlider);
                so.FindProperty("volumeSlider").objectReferenceValue = volumeSlider.GetComponent<Slider>();
                so.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;
            EditorGUILayout.Space();
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space();

            EditorUtil.ConditionalButton(new GUIContent("Set Slider to this"),
                targets.Cast<MusicVolumeSlider>().Where(a => a.VolumeSlider == null && a.GetComponent<Slider>() != null),
                SetSliderToThis);

            GUILayout.Label("Automatically sets up the OnValueChanged listener on the given Slider upon "
                + "entering play mode and on build.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            GUILayout.Label("Automatically makes the slider value match the Volume defined on the Manager, "
                + "similarly upon entering play mode and on build.",
                EditorStyles.wordWrappedLabel);
            // TODO: Add a utility to automatically remove stray listeners. Though that should be in the JanSharp Common package...
            // EditorGUILayout.Space();
            // GUILayout.Label("Use 'Tools -> JanSharp -> Remove UI Toggle Listeners Targeting Missing Objects' "
            //     + "to remove stray listeners on UI Toggles after deleting a UI Toggle Group Sync script.",
            //     EditorStyles.wordWrappedLabel);
        }
    }
}
