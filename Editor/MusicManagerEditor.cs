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
            EditorUtil.SetArrayProperty(
                musicManagerProxy.FindProperty("descriptors"),
                musicManager.GetComponentsInChildren<MusicDescriptor>(),
                (p, v) => p.objectReferenceValue = v
            );
            musicManagerProxy.ApplyModifiedProperties();

            return true;
        }
    }
}
