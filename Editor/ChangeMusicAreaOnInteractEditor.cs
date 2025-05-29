using UnityEngine;
using UnityEditor;
using System.Linq;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class ChangeMusicAreaOnInteractOnBuild
    {
        static ChangeMusicAreaOnInteractOnBuild()
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
