using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BgSlide : MonoBehaviour
{


    void Start()
    {
        StartCoroutine("Scroll");
    }

    IEnumerator Scroll()
    {
        const float scrollSpeed = 0.005f;

        RawImage rawImage = GetComponent<RawImage>();
        Rect uvRect = rawImage.uvRect;

        int time = 0;

        while (true)
        {
            float x = Mathf.Repeat(time * scrollSpeed, 1);
            time++;
            uvRect.x = x;
            rawImage.uvRect = uvRect;
            yield return new WaitForSeconds(0.025f);
        }
    }



}