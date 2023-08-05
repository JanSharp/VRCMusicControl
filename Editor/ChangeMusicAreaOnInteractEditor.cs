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
    public static class ChangeMusicAreaOnInteractOnBuild
    {
        static ChangeMusicAreaOnInteractOnBuild()
            => JanSharp.OnBuildUtil.RegisterType<ChangeMusicAreaOnInteract>(OnBuild);

        private static bool OnBuild(ChangeMusicAreaOnInteract changeMusicAreaOnInteract)
        {
            if (changeMusicAreaOnInteract.targets.Any(a => a == null))
            {
                Debug.LogWarning($"[MusicControl] {nameof(ChangeMusicAreaOnInteract)} "
                    + $"{changeMusicAreaOnInteract.name} contains a null target.",
                    changeMusicAreaOnInteract);
            }

            return true;
        }
    }
}
