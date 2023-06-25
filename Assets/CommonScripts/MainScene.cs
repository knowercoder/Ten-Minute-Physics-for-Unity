using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    public void ExittoHome()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadScene(string SceneName)
    {        
        SceneManager.LoadScene(SceneName);
    }

}
