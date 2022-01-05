using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class s_sceneTrans : MonoBehaviour
{
    public string sceneName;
    void Start()
    {
        SceneManager.LoadScene(sceneName);
    }
}
