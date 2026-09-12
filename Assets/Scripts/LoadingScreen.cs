using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Image loadingSlider;
    private void Start()
    {
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync("GameScene");

        loadOperation.allowSceneActivation = false;

        while (!loadOperation.isDone)
        {
            loadingSlider.fillAmount = loadOperation.progress;
            
            Debug.Log(loadOperation.progress);

            if (loadOperation.progress >= 0.9f)
            {
                
                yield return new WaitForSeconds(1f);
                    
                loadOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}