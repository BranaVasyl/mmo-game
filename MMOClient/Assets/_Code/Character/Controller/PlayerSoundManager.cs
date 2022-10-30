using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class PlayerSoundManager : MonoBehaviour
    {
        [SerializeField]
        private AudioClip[] stepClips;

        [SerializeField]
        private AudioClip rollClip;

        private AudioSource audioSource;

        private StateManager states;

        public void Init(StateManager st)
        {
            states = st;
            audioSource = GetComponent<AudioSource>();
        }

        public void Roll()
        {
            audioSource.PlayOneShot(rollClip);
        }

        public void Step(string animationType)
        {
            switch (animationType)
            {
                case "run":
                    if (!states.run)
                    {
                        return;
                    }
                    break;
                case "jog":
                    if (states.run || states.walk)
                    {
                        return;
                    }
                    break;
                case "walk":
                    if (!states.walk)
                    {
                        return;
                    }
                    break;
                default:
                    return;
            }

            if (animationType == "run" || animationType == "jog" || animationType == "walk")
            {
                if (states.moveAmount == 0 || !states.canMove)
                {
                    return;
                }
            }

            AudioClip clip = GetRandomClip();

            if (clip)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private AudioClip GetRandomClip()
        {
            if (stepClips.Length == 0)
            {
                return null;
            }

            return stepClips[UnityEngine.Random.Range(0, stepClips.Length)];
        }
    }
}
