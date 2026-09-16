using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeController : MonoBehaviour
{
    private Animator animator;
    private MenuScript menuScript;
    void Start()
    {
        menuScript = FindObjectOfType<MenuScript>();
        animator = GetComponent<Animator>();
        FadeIn();
    }

    public void FadeIn()
    { animator.SetTrigger("FadeIn"); }
    
    public void FadeOut()
    { animator.SetTrigger("FadeOut"); }

    public void StartGame() { StartCoroutine(startG());}
    
    public void QuitGame() { StartCoroutine(quitG()); }
    
    private IEnumerator startG()
    {
        animator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(0.4f);
        menuScript.StartGame();
    }

    private IEnumerator quitG()
    {
        animator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(0.3f);
        menuScript.QuitGame();
    }

    public IEnumerator backMenu()
    {
        animator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene("EndGame");
    }
    
    
    
}
