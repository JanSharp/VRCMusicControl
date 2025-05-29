using UdonSharp;
using UnityEngine;
using JetBrains.Annotations;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    /// <summary>
    /// <para>The API can be used in Awake, OnEnable or Start. It will be initialized in time.</para>
    /// </summary>
    public class MusicAreaToggleOnInteract : UdonSharpBehaviour
    {
        [Tooltip("The MusicArea to toggle the Is Active state on. Must not be null.")]
        [SerializeField] private MusicArea areaToToggle;
        [PublicAPI] public MusicArea AreaToToggle
        {
            get => areaToToggle;
            set
            {
                if (value == areaToToggle)
                    return;
                if (value == null)
                {
                    Debug.LogError($"[MusicControl] Attempt to set the {nameof(AreaToToggle)} to null on the "
                        + $"{nameof(MusicAreaToggleOnInteract)} {this.name}, ignoring.", this);
                    return;
                }
                areaToToggle = value;
            }
        }

        [PublicAPI] public override void Interact() => areaToToggle.IsActive = !areaToToggle.IsActive;
    }
}
