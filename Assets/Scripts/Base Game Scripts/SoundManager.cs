using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource[] destroyNoise;
    public AudioSource backgroundMusic;

    private void Start() {
        if(PlayerPrefs.HasKey("Sound")) {
            if(PlayerPrefs.GetInt("Sound") == 0) {
                backgroundMusic.Play();
                backgroundMusic.volume = 0;
            } else {
                backgroundMusic.Play();
                backgroundMusic.volume = 1;
            }
        } else {
            backgroundMusic.Play();
            backgroundMusic.volume = 1;
        }
        Debug.Log("backgroundMusic.volume = " + backgroundMusic.volume);
    }

    public void AdjustVolumn() {
        if(PlayerPrefs.HasKey("Sound")) {
            if(PlayerPrefs.GetInt("Sound") == 0) {
                backgroundMusic.volume = 0;
            } else {
                backgroundMusic.volume = 1;
            }
        }
    }

    public void PlayRandomDestroyNoise() {
        if(PlayerPrefs.HasKey("Sound")) {
            if(PlayerPrefs.GetInt("Sound") == 1) {
                PlaySound();
            }
        } else {
            PlaySound();
        }
    }

    private void PlaySound(){
        //Choose a random number
        int clipToPlay = Random.Range(0, destroyNoise.Length);
        //play clip
        destroyNoise[clipToPlay].Play();
    }
}
