using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private RawImage _img;
    [SerializeField] private float _xSpeed = 0.1f;
    [SerializeField] private float _ySpeed = 0.05f;

    void Update()
    {
        // We calculate the new rectangle position
        // uvRect.position is the offset (where the texture starts)
        // uvRect.size is the tiling (how many times it repeats)
        Rect rect = _img.uvRect;
        
        rect.x += _xSpeed * Time.deltaTime;
        rect.y += _ySpeed * Time.deltaTime;
        
        _img.uvRect = rect;
    }
}