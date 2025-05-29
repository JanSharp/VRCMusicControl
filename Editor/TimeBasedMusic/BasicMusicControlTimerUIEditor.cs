using UnityEngine;
using UnityEditor;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class BasicMusicControlTimerUIOnBuild
    {
        static BasicMusicControlTimerUIOnBuild()
            => OnBuildUtil.RegisterType<BasicMusicControlTimerUI>(OnBuild);

        private static bool OnBuild(BasicMusicControlTimerUI basicMusicControlTimerUI)
        {
            SerializedObject proxy = new SerializedObject(basicMusicControlTimerUI);

            if (proxy.FindProperty("sharedTimer").objectReferenceValue == null)
            {
                Debug.LogError($"[MusicControl] The Shared Timer must not be null for "
                    + $"{basicMusicControlTimerUI.name}.", basicMusicControlTimerUI);
                return false;
            }

            return true;
        }
    }
}
