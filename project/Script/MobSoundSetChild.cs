using UnityEngine;

namespace Atavism
{

    public class MobSoundSetChild : MonoBehaviour
    {

        public void PlaySoundEvent(MobSoundEvent soundEvent)
        {
            MobSoundSet mss = transform.parent.GetComponent<MobSoundSet>();
            if (mss != null)
                mss.PlaySoundEvent(soundEvent);
        }

        public void DidJump()
        {
            PlaySoundEvent(MobSoundEvent.Jump);
        }

        public void PlaySound(string name)
        {
            switch (name)
            {
                case "jump":
                    PlaySoundEvent(MobSoundEvent.Jump);
                    break;
                case "response":
                    PlaySoundEvent(MobSoundEvent.Response);
                    break;
                case "death":
                    PlaySoundEvent(MobSoundEvent.Death);
                    break;
                case "aggro":
                    PlaySoundEvent(MobSoundEvent.Aggro);
                    break;
                case "attack":
                    PlaySoundEvent(MobSoundEvent.Attack);
                    break;
            }
        }
    }

}