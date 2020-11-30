using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    Animator transition;

    float transitionTime = 1f;

    void Awake()
    {
        transition = gameObject.GetComponentInChildren<Animator>(); 
    }

    public IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("startTransition");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelIndex);
    }
}
