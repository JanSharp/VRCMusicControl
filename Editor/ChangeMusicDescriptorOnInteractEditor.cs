using UnityEngine;
using UnityEditor;
using System.Linq;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class ChangeMusicDescriptorOnInteractOnBuild
    {
        static ChangeMusicDescriptorOnInteractOnBuild()
            => OnBuildUtil.RegisterType<ChangeMusicDescriptorOnInteract>(OnBuild);

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
