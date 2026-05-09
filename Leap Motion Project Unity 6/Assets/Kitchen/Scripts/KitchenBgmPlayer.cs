using UnityEngine;

namespace KitchenGame
{
    [RequireComponent(typeof(AudioSource))]
    public class KitchenBgmPlayer : MonoBehaviour
    {
        private static KitchenBgmPlayer instance;

        [SerializeField]
        private AudioClip bgmClip;

        [SerializeField]
        [Range(0f, 1f)]
        private float volume = 0.42f;

        [SerializeField]
        private bool loop = true;

        [SerializeField]
        private bool playOnStart = true;

        [SerializeField]
        private bool dontDestroyOnLoad = true;

        private AudioSource audioSource;

        private void Awake()
        {
            if (dontDestroyOnLoad)
            {
                if (instance != null && instance != this)
                {
                    Destroy(gameObject);
                    return;
                }

                instance = this;
                DontDestroyOnLoad(gameObject);
            }

            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = loop;
            audioSource.volume = volume;
            audioSource.clip = bgmClip;
        }

        private void Start()
        {
            if (playOnStart)
            {
                Play();
            }
        }

        private void OnValidate()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                return;
            }

            audioSource.loop = loop;
            audioSource.volume = volume;
            audioSource.clip = bgmClip;
        }

        public void Play()
        {
            if (audioSource == null || bgmClip == null)
            {
                return;
            }

            if (audioSource.clip != bgmClip)
            {
                audioSource.clip = bgmClip;
            }

            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        public void Stop()
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.Stop();
        }
    }
}
