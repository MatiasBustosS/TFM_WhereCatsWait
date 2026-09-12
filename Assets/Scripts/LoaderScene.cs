using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scenes
{
    GameScene,
    MenuScene
}

public class LoaderScene : MonoBehaviour
{
    [SerializeField] private Scenes _scene;

    public void LoadScene()
    {
        SceneManager.LoadScene(_scene.ToString());
    }
}
