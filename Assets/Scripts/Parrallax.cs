using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Range(0f, 1f)] public float followX = 0.8f;
    [Range(0f, 1f)] public float followY = 0.8f;
    Transform cam;
    Vector3 startPos;
    Vector3 camStartPos;

    void Start()
    {
        cam = Camera.main.transform;
        startPos = transform.position;
        camStartPos = cam.position;
    }

    void LateUpdate() // runs after the camera has moved this frame
    {
        Vector3 camMoved = cam.position - camStartPos;
        transform.position = startPos + new Vector3(camMoved.x * followX, camMoved.y * followY, 0f);
    }
}