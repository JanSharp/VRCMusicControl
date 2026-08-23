using System.Linq;
using UnityEngine;

namespace JanSharp
{
    public static class ChangeMusicDescriptorOnInteractOnBuild
    {
        [OrderedInitializeOnLoad]
        private static void OnAssemblyLoad()
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
