using System.Linq;
using UnityEngine;

namespace JanSharp
{
    public static class ChangeMusicAreaOnInteractOnBuild
    {
        [OrderedInitializeOnLoad]
        private static void OnAssemblyLoad()
            => OnBuildUtil.RegisterType<ChangeMusicAreaOnInteract>(OnBuild);

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
