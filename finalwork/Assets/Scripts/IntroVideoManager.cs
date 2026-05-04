using UnityEngine;
using UnityEngine.Video;

public class IntroVideoManager : MonoBehaviour
{
    public GameObject introVideo; 

    private VideoPlayer video;

    void Start()
    {
        video = introVideo.GetComponent<VideoPlayer>();
        video.loopPointReached += EndVideo;
    }

    void EndVideo(VideoPlayer vp)
    {
        introVideo.SetActive(false);
    }
}