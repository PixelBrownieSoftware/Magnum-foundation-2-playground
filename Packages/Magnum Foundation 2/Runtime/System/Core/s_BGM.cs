using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagnumFoundation2.System.Core;

public class s_BGM : s_singleton<s_BGM>
{
    public AudioClip music;
    public AudioSource src;
    public float BGMvolume = 1;

    bool isBGMVolume = true;

    public void PlaySong(AudioClip song)
    {
        if (music == song)
            return;
        music = song;
        src.loop = true;
        src.clip = song;
        src.Play();
    }
    public void PlaySong(string nameOfSong) {
        AudioClip clp = s_soundmanager.GetInstance().GetClip(nameOfSong);
        if (music == clp)
            return;
        src.loop = true;
        src.clip = clp;
        src.Play();
    }
    public IEnumerator FadeInMusic(AudioClip song)
    {
        if (music == song)
            yield return null;
        isBGMVolume = false;
        src.volume = 0;
        src.loop = true;
        src.clip = song;
        src.Play();
        while (src.volume < BGMvolume)
        {
            src.volume += Time.unscaledDeltaTime;
            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        }
        isBGMVolume = true;
        src.volume = BGMvolume;
    }
    public IEnumerator FadeInMusic(AudioClip song, float speed)
    {
        if (music == song)
            yield return null;
        isBGMVolume = false;
        src.volume = 0;
        src.clip = song;
        src.Play();
        src.loop = true;
        while (src.volume < BGMvolume)
        {
            src.volume += Time.unscaledDeltaTime * speed;
            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        }
        isBGMVolume = true;
        src.volume = BGMvolume;
    }
    public IEnumerator FadeInMusic()
    {
        isBGMVolume = false;
        while (src.volume < BGMvolume)
        {
            src.volume += Time.unscaledDeltaTime;
            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        }
        isBGMVolume = true;
        src.loop = true;
        src.volume = BGMvolume;
    }
    public IEnumerator FadeOutMusic(float speed)
    {
        isBGMVolume = false;
        while (src.volume > 0)
        {
            src.volume -= Time.unscaledDeltaTime * speed;
            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        }
        isBGMVolume = true;
        StopSong();
        src.loop = true;
        src.volume = BGMvolume;
    }

    public IEnumerator FadeOutMusic()
    {
        isBGMVolume = false;
        while (src.volume > 0) {
            src.volume -= Time.unscaledDeltaTime;
            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime );
        }
        isBGMVolume = true;
        StopSong();
        src.loop = true;
        src.volume = BGMvolume;
    }

    private void Update()
    {
        if (isBGMVolume) {
            src.volume = BGMvolume;
        }
    }

    public void StopSong() {
        music = null;
        src.Stop();
    }
}
