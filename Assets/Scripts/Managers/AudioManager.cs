using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource _sfxSource;

    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioClip[] _songs;
    
    private int _currentSongIndex = 0;
    

    private void Awake() {
        Instance = this;
    }
    private void Start() {
        PlaySong(_currentSongIndex);
    }

    private void Update() {
        if (!_musicSource.isPlaying)
        {
            NextSong();
        }
    }

    private void PlaySong(int index)
    {
        if (_songs.Length == 0) return;

        _musicSource.clip = _songs[index];
        _musicSource.Play();
    }

    private void NextSong()
    {
        _currentSongIndex++;

        if (_currentSongIndex >= _songs.Length)
        {
            _currentSongIndex = 0;
        }

        PlaySong(_currentSongIndex);
    }

    public void PlaySFX(AudioClip clip)
    {
        _sfxSource.PlayOneShot(clip);
    }
}
