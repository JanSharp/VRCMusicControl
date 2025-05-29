using UnityEngine;
using VRC.SDK3.Components;
using UnityEditor;
using UdonSharpEditor;
using System.Linq;
using System.Reflection;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class MusicDescriptorOnBuild
    {
        static MusicDescriptorOnBuild() => OnBuildUtil.RegisterType<MusicDescriptor>(OnBuild);

        private static bool OnBuild(MusicDescriptor musicDescriptor)
        {
            if (musicDescriptor.GetComponentInParent<MusicManager>() == null)
            {
                Debug.LogError($"[MusicControl] {nameof(MusicDescriptor)} {musicDescriptor.name} "
                    + $"must be a child of a {nameof(MusicManager)}.", musicDescriptor);
                return false;
            }

            return true;
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(MusicDescriptor))]
    public class MusicDescriptorEditor : Editor
    {
        private static bool GetIsSilenceDescriptor(MusicDescriptor descriptor)
        {
            return (bool)typeof(MusicDescriptor)
                .GetField("isSilenceDescriptor", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(descriptor);
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;
            EditorGUILayout.Space();
            base.OnInspectorGUI(); // draws public/serializable fields
            EditorGUILayout.Space();

            EditorUtil.ConditionalButton(
                new GUIContent("Add Audio Source", "An Audio Source is required for non silence descriptors."),
                targets.Cast<MusicDescriptor>()
                    .Where(d => !GetIsSilenceDescriptor(d) && d.GetComponent<AudioSource>() == null),
                descriptors =>
                {
                    foreach (MusicDescriptor descriptor in descriptors)
                        descriptor.gameObject.AddComponent<AudioSource>();
                }
            );

            EditorUtil.ConditionalButton(
                new GUIContent("Disable Play On Awake",
                    "The system requires Play On Awake to be disabled (including on the default descriptor).\n"
                        + "(Play On Awake gets disabled automatically when entering play mode or publishing.)"
                ),
                targets.Cast<MusicDescriptor>()
                    .Where(d => !GetIsSilenceDescriptor(d))
                    .Select(d => d.GetComponent<AudioSource>())
                    .Where(a => a != null && a.playOnAwake),
                audioSources =>
                {
                    SerializedObject audioSourceProxy = new SerializedObject(audioSources.ToArray());
                    audioSourceProxy.FindProperty("m_PlayOnAwake").boolValue = false;
                    audioSourceProxy.ApplyModifiedProperties();
                }
            );

            EditorUtil.ConditionalButton(
                new GUIContent("Enable Loop",
                    "The system requires looping audio sources.\n"
                        + "(Loop gets enabled automatically when entering play mode or publishing.)"
                ),
                targets.Cast<MusicDescriptor>()
                    .Where(d => !GetIsSilenceDescriptor(d))
                    .Select(d => d.GetComponent<AudioSource>())
                    .Where(a => a != null && !a.loop),
                audioSources =>
                {
                    SerializedObject audioSourceProxy = new SerializedObject(audioSources.ToArray());
                    audioSourceProxy.FindProperty("Loop").boolValue = true;
                    audioSourceProxy.ApplyModifiedProperties();
                }
            );

            EditorUtil.ConditionalButton(
                new GUIContent("Add VRC Spatial Audio Source?",
                    "Pretty sure these are essentially required for VRChat, but I could be wrong."
                ),
                targets.Cast<MusicDescriptor>()
                    .Where(d => !GetIsSilenceDescriptor(d) && d.GetComponent<VRCSpatialAudioSource>() == null),
                descriptors =>
                {
                    foreach (MusicDescriptor descriptor in descriptors)
                        descriptor.gameObject.AddComponent<VRCSpatialAudioSource>();
                }
            );

            ///cSpell:ignore Spatialization

            EditorUtil.ConditionalButton(
                new GUIContent("Disable Spatialization on VRC Spatial Audio Source?",
                    "The system is made for 2D/directionless audio, so I believe this should be disabled. "
                        + "I could be wrong."
                ),
                targets.Cast<MusicDescriptor>()
                    .Where(d => !GetIsSilenceDescriptor(d))
                    .Select(d => d.GetComponent<VRCSpatialAudioSource>())
                    .Where(s => s != null && s.EnableSpatialization),
                vrcAudioSources =>
                {
                    SerializedObject audioSourceProxy = new SerializedObject(vrcAudioSources.ToArray());
                    audioSourceProxy.FindProperty("EnableSpatialization").boolValue = false;
                    audioSourceProxy.ApplyModifiedProperties();
                }
            );
        }
    }
}
