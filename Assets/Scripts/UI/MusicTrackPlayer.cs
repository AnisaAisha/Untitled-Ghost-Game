using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicTrackPlayer : MonoBehaviour
{
    [SerializeField] private Image spinningDisc;
    [SerializeField] private TextMeshProUGUI nowPlayingText;
    [SerializeField] private float discRotationSpeed = 180f;
    void Start()
    {
        if (nowPlayingText == null || spinningDisc == null)
        {
            Debug.LogError($"{name} is missing references to UI elements");
        }
        ArcEvent.OnArcChanged += ChangeArcMusic;
    }
    private void OnEnable()
    {
        AudioManager.OnSongChanged += UpdateSongName;
    }

    private void OnDisable()
    {
        AudioManager.OnSongChanged -= UpdateSongName;
    }

    private void Update()
    {
        spinningDisc.transform.Rotate(0f, 0f, Time.deltaTime * -discRotationSpeed);
    }
    private void UpdateSongName(string songName)
    {
        nowPlayingText.text = $"Now Playing: {songName}";
    }

    public void ChangeSong()
    {
        // AudioManager.Instance.RandomSong();
        AudioManager.Instance.NextSong();
    }

    public void ChangeSong(int index)
    {
        AudioManager.Instance.NextSong(index);
    }
    private void ChangeArcMusic()
    {
        // subtract 1 for the the beginning arc
        ChangeSong((int) GameManager.Instance.arc - 1);
    }
}
