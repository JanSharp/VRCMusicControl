using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEditor;
using UdonSharpEditor;
using System.Linq;
using System.Collections.Generic;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class MusicManagerOnBuild
    {
        static MusicManagerOnBuild() => JanSharp.OnBuildUtil.RegisterType<MusicManager>(OnBuild);

        private static bool OnBuild(MusicManager musicManager)
        {
            // Using this weird logic with Linq because GetComponentInChildren includes self.
            MusicManager nestedManager = musicManager.transform.Cast<Transform>()
                .Select(t => t.GetComponentInChildren<MusicManager>(true))
                .FirstOrDefault(m => m != null);
            if (nestedManager != null)
            {
                Debug.LogError($"[MusicControl] {nameof(MusicManager)}s must "
                    + $"not be nested inside of each other", nestedManager);
                return false;
            }

            SerializedObject musicManagerProxy = new SerializedObject(musicManager);
            MusicDescriptor[] descriptors = musicManager.GetComponentsInChildren<MusicDescriptor>(true);
            EditorUtil.SetArrayProperty(
                musicManagerProxy.FindProperty("descriptors"),
                descriptors,
                (p, v) => p.objectReferenceValue = v
            );
            musicManagerProxy.FindProperty("syncGlobalStartTime").boolValue
                = descriptors.Any(d => d.StartType == MusicStartType.GlobalTimeSinceWorldStartSynced);
            musicManagerProxy.ApplyModifiedProperties();

            List<AudioSource> audioSources = new List<AudioSource>();

            int i = 0;
            foreach (MusicDescriptor descriptor in descriptors)
            {
                SerializedObject descriptorProxy = new SerializedObject(descriptor);
                descriptorProxy.FindProperty("manager").objectReferenceValue = musicManager;
                descriptorProxy.FindProperty("index").intValue = i++;
                descriptorProxy.ApplyModifiedProperties();

                if (descriptorProxy.FindProperty("isSilenceDescriptor").boolValue)
                {
                    descriptorProxy.ApplyModifiedProperties();
                    continue;
                }

                AudioSource audioSource = descriptor.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    Debug.LogError($"[MusicControl] {nameof(MusicDescriptor)} {descriptor.name} "
                        + $"is missing an AudioSource component.", descriptor);
                    return false;
                }
                audioSources.Add(audioSource);
                descriptorProxy.FindProperty("audioSource").objectReferenceValue = audioSource;
                descriptorProxy.FindProperty("forcedSilenceDescriptor").boolValue = audioSource.clip == null;
                descriptorProxy.FindProperty("maxVolume").floatValue = audioSource.volume;
                descriptorProxy.FindProperty("firstFadeInInterval").floatValue
                    = MusicDescriptor.CalculateUpdateInterval(descriptor.FirstFadeInSeconds);
                descriptorProxy.FindProperty("fadeInInterval").floatValue
                    = MusicDescriptor.CalculateUpdateInterval(descriptor.FadeInSeconds);
                descriptorProxy.FindProperty("fadeOutInterval").floatValue
                    = MusicDescriptor.CalculateUpdateInterval(descriptor.FadeOutSeconds);

                descriptorProxy.ApplyModifiedProperties();
            }

            // NOTE: When changing this logic, remember that there are tooltips about this for buttons in the
            // MusicDescriptorEditor file.
            var unfinishedAudioSources = audioSources.Where(a => a.playOnAwake || !a.loop);
            if (unfinishedAudioSources.Any())
            {
                SerializedObject audioSourceProxy = new SerializedObject(unfinishedAudioSources.ToArray());
                audioSourceProxy.FindProperty("m_PlayOnAwake").boolValue = false;
                audioSourceProxy.FindProperty("Loop").boolValue = true;
                audioSourceProxy.ApplyModifiedProperties();
            }

            if (musicManager.DefaultMusic != null
                && !descriptors.Contains(musicManager.DefaultMusic))
            {
                Debug.LogError($"[MusicControl] {nameof(MusicManager)}'s Default Music must be managed by "
                    + $"this music manager, aka it must be a child of it.", musicManager);
                return false;
            }

            return true;
        }
    }
}
