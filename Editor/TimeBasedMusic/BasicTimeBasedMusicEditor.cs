using UnityEngine;
using UnityEditor;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class BasicTimeBasedMusicOnBuild
    {
        static BasicTimeBasedMusicOnBuild()
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
