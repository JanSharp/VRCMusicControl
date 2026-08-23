using UnityEditor;
using UnityEngine;

namespace JanSharp
{
    public static class BasicTimeBasedMusicOnBuild
    {
        [OrderedInitializeOnLoad]
        private static void OnAssemblyLoad()
            => OnBuildUtil.RegisterType<BasicTimeBasedMusic>(OnBuild);

        private static bool OnBuild(BasicTimeBasedMusic basicTimeBasedMusic)
        {
            SerializedObject proxy = new SerializedObject(basicTimeBasedMusic);

            if (proxy.FindProperty("sharedTimer").objectReferenceValue == null)
            {
                Debug.LogError($"[MusicControl] The Shared Timer must not be null for "
                    + $"{basicTimeBasedMusic.name}.", basicTimeBasedMusic);
                return false;
            }

            return true;
        }
    }
}
