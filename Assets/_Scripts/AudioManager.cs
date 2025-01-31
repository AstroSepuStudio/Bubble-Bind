using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioSource _audioSourceBGM;
    [SerializeField] AudioData _bgm;

    private void Awake()
    {
        transform.parent = null;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        _audioSourceBGM.clip = _bgm.Clip;
        _audioSource.volume = _bgm.Volume;
        _audioSourceBGM.Play();
    }

    public void PlayClip(AudioData audio)
    {
        _audioSource.volume = audio.Volume;
        _audioSource.PlayOneShot(audio.Clip);
    }
}
