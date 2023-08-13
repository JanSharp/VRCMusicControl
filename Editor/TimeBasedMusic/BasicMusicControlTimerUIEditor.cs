using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using UnityEditor;
using UdonSharpEditor;
using System.Linq;
using System.Collections.Generic;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class BasicMusicControlTimerUIOnBuild
    {
        static BasicMusicControlTimerUIOnBuild()
            => JanSharp.OnBuildUtil.RegisterType<BasicMusicControlTimerUI>(OnBuild);

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
