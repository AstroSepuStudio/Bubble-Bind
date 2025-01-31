using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class AudioVolumeTest : MonoBehaviour
{
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] AudioSource _audioSource;
    [SerializeField] TextMeshProUGUI _audioText;
    [SerializeField] int _index;
    [SerializeField] AudioData[] _audioDatas;

    private void Start()
    {
        _playerInput.actions["Test"].started += ctx => PlayAudio();
        _playerInput.actions["Move"].performed += ctx => CycleInventory();
        _audioText.SetText(_audioDatas[_index].name);
    }

    void CycleInventory()
    {
        if (_playerInput.actions["Move"].ReadValue<Vector2>().x > 0)
            CycleInventoryDown();
        else if (_playerInput.actions["Move"].ReadValue<Vector2>().x < 0)
            CycleInventoryUp();
    }

    void CycleInventoryUp()
    {
        _index++;
        if (_index >= _audioDatas.Length)
            _index = 0;

        _audioText.SetText(_audioDatas[_index].name);
    }

    void CycleInventoryDown()
    {
        _index--;
        if (_index < 0)
            _index = _audioDatas.Length - 1;

        _audioText.SetText(_audioDatas[_index].name);
    }

    void PlayAudio()
    {
        if (_audioSource.isPlaying) 
            _audioSource.Stop();

        _audioSource.volume = _audioDatas[_index].Volume;
        _audioSource.clip = _audioDatas[_index].Clip;
        _audioSource.Play();
    }
}
