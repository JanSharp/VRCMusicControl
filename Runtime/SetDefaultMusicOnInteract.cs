using UdonSharp;
using JetBrains.Annotations;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    /// <summary>
    /// <para>The API can be used in Awake, OnEnable or Start. It will be initialized in time.</para>
    /// </summary>
    public class SetDefaultMusicOnInteract : UdonSharpBehaviour
    {
        [PublicAPI] public MusicDescriptor musicToSwitchTo;

        [PublicAPI] public override void Interact() => musicToSwitchTo.SetAsDefault();
    }
}
