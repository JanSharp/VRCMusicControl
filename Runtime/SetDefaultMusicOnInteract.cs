using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using JetBrains.Annotations;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SetDefaultMusicOnInteract : UdonSharpBehaviour
    {
        [PublicAPI] public MusicDescriptor musicToSwitchTo;

        [PublicAPI] public override void Interact() => musicToSwitchTo.SetAsDefault();
    }
}
