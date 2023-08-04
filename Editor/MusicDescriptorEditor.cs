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
    public static class MusicDescriptorOnBuild
    {
        static MusicDescriptorOnBuild() => JanSharp.OnBuildUtil.RegisterType<MusicDescriptor>(OnBuild);

        private static bool OnBuild(MusicDescriptor musicDescriptor)
        {
            if (musicDescriptor.GetComponentInParent<MusicManager>() == null)
            {
                Debug.LogError($"[MusicControl] {nameof(MusicDescriptor)} {musicDescriptor.name} "
                    + $"must be a child of a {nameof(MusicManager)}.", musicDescriptor);
                return false;
            }

            return true;
        }
    }
}
