using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class s_intro : MonoBehaviour
{
    public Animator anim;
    public string title = "Title";
    public bool noIntro = true;
    public AudioSource audioSrc;

    public void SwitchToTitle() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(title);
    }

    public void PlayIntroSound() {
        audioSrc.Play();
    }

    private void Update()
    {
        if (noIntro)
            SwitchToTitle();
    }
}
