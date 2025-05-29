using UnityEngine;
using UnityEditor;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class MusicAreaToggleOnInteractOnBuild
    {
        static MusicAreaToggleOnInteractOnBuild()
            => OnBuildUtil.RegisterType<MusicAreaToggleOnInteract>(OnBuild);

        private static bool OnBuild(MusicAreaToggleOnInteract musicAreaToggleOnInteract)
        {
            SerializedObject proxy = new SerializedObject(musicAreaToggleOnInteract);

            if (proxy.FindProperty("areaToToggle").objectReferenceValue == null)
            {
                Debug.LogError($"[MusicControl] The Music Area To Toggle must not be null for "
                    + $"{musicAreaToggleOnInteract.name}.", musicAreaToggleOnInteract);
                return false;
            }

            return true;
        }
    }
}
