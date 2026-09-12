using System.Collections;
using UnityEngine;

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

    void FadeIn()
    { animator.SetTrigger("FadeIn"); }

    public void StartGame() { StartCoroutine(startG());}
    
    public void QuitGame() { StartCoroutine(quitG()); }
    
    public IEnumerator startG()
    {
        animator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(0.4f);
        menuScript.StartGame();
    }

    public IEnumerator quitG()
    {
        animator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(0.3f);
        menuScript.QuitGame();
    }
    
    
    
}
