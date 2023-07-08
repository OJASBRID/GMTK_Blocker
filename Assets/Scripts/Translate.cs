using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translate : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject gm;
    public Vector3 pf;
    
    private float scaleFactor = 1;
    void Start()
    {
        Bounds parentBounds = GetBounds(transform);

        Camera camera = Camera.main;
        float screenHeight = 2f * camera.orthographicSize;

        scaleFactor = screenHeight / parentBounds.size.y;

        transform.localScale = new Vector3(
            transform.localScale.x * scaleFactor,
            transform.localScale.y * scaleFactor,
            transform.localScale.z
        );
    }

    // Update is called once per frame
    void Update()
    {
        gm.transform.position = pf * scaleFactor;
    }
    private Bounds GetBounds(Transform transform)
    {
        Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(transform.position, Vector3.zero);

        foreach (Renderer renderer in renderers)
            bounds.Encapsulate(renderer.bounds);

        return bounds;
    }
    private Bounds GetWindowBounds()
    {
        Camera camera = Camera.main;
        float distance = camera.transform.position.z * -1;
        float height = 2f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * camera.aspect;
        Vector3 center = camera.transform.position + camera.transform.forward * distance;
        Vector3 size = new Vector3(width, height, 0f);
        Bounds bounds = new Bounds(center, size);

        return bounds;
    }
}
