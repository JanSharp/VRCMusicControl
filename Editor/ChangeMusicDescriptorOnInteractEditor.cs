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
    public static class ChangeMusicDescriptorOnInteractOnBuild
    {
        static ChangeMusicDescriptorOnInteractOnBuild()
            => JanSharp.OnBuildUtil.RegisterType<ChangeMusicDescriptorOnInteract>(OnBuild);

        private static bool OnBuild(ChangeMusicDescriptorOnInteract changeMusicDescriptorOnInteract)
        {
            if (changeMusicDescriptorOnInteract.targets.Any(d => d == null))
            {
                Debug.LogWarning($"[MusicControl] {nameof(ChangeMusicDescriptorOnInteract)} "
                    + $"{changeMusicDescriptorOnInteract.name} contains a null target.",
                    changeMusicDescriptorOnInteract);
            }

            return true;
        }
    }
}
