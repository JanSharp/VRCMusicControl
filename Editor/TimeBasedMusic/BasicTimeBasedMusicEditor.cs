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
    public static class BasicTimeBasedMusicOnBuild
    {
        static BasicTimeBasedMusicOnBuild()
            => JanSharp.OnBuildUtil.RegisterType<BasicTimeBasedMusic>(OnBuild);

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
