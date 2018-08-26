using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BgSlide : MonoBehaviour
{

    private float scrollSpeed = 1;
    private Rect uvrect;
    private float time = 0;


    void Start()
    {
        uvrect = GetComponent<RawImage>().uvRect;
        StartCoroutine("Scroll");
    }

    IEnumerator Scroll()
    {
        while (true)
        {
            float x = Mathf.Repeat(time * 0.1f * scrollSpeed, 1);
            time += 0.02f;
            uvrect.x = x;
            GetComponent<RawImage>().uvRect = uvrect;
            yield return new WaitForSeconds(0.025f);
        }
    }



}
