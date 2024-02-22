using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicLoop : MonoBehaviour
{
    [SerializeField]
    private AudioSource musicLoop1, musicLoop2;

    [SerializeField]
    private UIManager uiManager;

    void Start()
    {
        StartCoroutine(PlayLoop1(50.84f));
    }

    private IEnumerator PlayLoop1(float delayInSeconds) {
        musicLoop1.Play();
        yield return new WaitForSeconds(delayInSeconds);
        StartCoroutine(PlayLoop2(50.84f));
    }

    private IEnumerator PlayLoop2(float delayInSeconds) {
        musicLoop2.Play();
        yield return new WaitForSeconds(delayInSeconds);
        StartCoroutine(PlayLoop1(50.84f));
    }

    private void Update() {
        if (uiManager.isGamePaused) {
            musicLoop1.volume = 0;
            musicLoop2.volume = 0;
        } else {
            musicLoop1.volume = 1;
            musicLoop2.volume = 1;
        }
    }
}
