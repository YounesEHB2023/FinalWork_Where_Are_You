using System.Collections;
using UnityEngine;

public class IntroTextManager : MonoBehaviour
{
    public CanvasGroup introGroup;
    public GameObject introUI;

    public float fadeInTime = 1f;
    public float stayTime = 2f;
    public float fadeOutTime = 1f;

    void Start()
    {
        introUI.SetActive(true);
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        introGroup.alpha = 0f;

        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            introGroup.alpha = t / fadeInTime;
            yield return null;
        }

        introGroup.alpha = 1f;

        yield return new WaitForSeconds(stayTime);

        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            introGroup.alpha = 1f - (t / fadeOutTime);
            yield return null;
        }

        introGroup.alpha = 0f;
        introUI.SetActive(false);
    }
}