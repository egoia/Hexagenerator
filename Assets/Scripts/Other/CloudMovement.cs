using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    public float speed;
    public Vector3 direction;
    public float distanceToCross;
    private Vector3 startPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += speed * Time.deltaTime * direction;
        if ((transform.position - startPosition).magnitude >= distanceToCross) Destroy(gameObject);
    }
}
