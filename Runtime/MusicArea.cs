using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MusicArea : UdonSharpBehaviour
    {
        public MusicDescriptor musicForThisArea;
        private uint id;
        private int triggerCount;

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                triggerCount++;
                if (triggerCount == 1)
                    id = musicForThisArea.AddThisMusic();
            }
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                if (triggerCount == 0)
                    return;
                triggerCount--;
                if (triggerCount == 0)
                    musicForThisArea.Manager.RemoveMusic(id);
            }
        }
    }
}
