using UnityEngine;

public class CloudMaker : MonoBehaviour
{
    public GameObject smallCloud;
    public GameObject bigCloud;
    public Vector2 size;
    public Vector2 speedRange;
    public Vector2 scaleRange = new Vector2(1, 2);
    public Vector2 spawnIntervalRange;
    public float destroyDistance;
    private float currentInterval;
    private float _timer = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PickInterval();
    }

    // Update is called once per frame
    void Update()
    {
        if (_timer > currentInterval)
        {
            _timer = 0;
            SpawnCloud();
            PickInterval();
        }
        _timer += Time.deltaTime;
    }

    void SpawnCloud()
    {
        float x = Random.Range(-size.x / 2, size.x / 2);
        float y = Random.Range(-size.y / 2, size.y / 2);
        GameObject prefab = Random.Range(0, 2) > 0 ? smallCloud : bigCloud;
        float speed = Random.Range(speedRange.x, speedRange.y);
        float scale = Random.Range(scaleRange.x, scaleRange.y);
        float rotation = Random.Range(0, 360);

        GameObject cloud = Instantiate(prefab, transform);
        cloud.transform.localPosition = new Vector3(x, y, 0);
        cloud.GetComponent<CloudMovement>().speed = speed;
        cloud.GetComponent<CloudMovement>().distanceToCross = destroyDistance;
        cloud.GetComponent<CloudMovement>().direction = transform.forward;
        cloud.transform.localScale *= scale;
        cloud.transform.rotation = Quaternion.Euler(0, rotation,0);
    }

    void PickInterval()
    {
        currentInterval = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3[] corners = new Vector3[4];
        corners[0] = transform.TransformPoint(new Vector3(-size.x / 2,  -size.y / 2,0));
        corners[1] = transform.TransformPoint(new Vector3(size.x / 2,  -size.y / 2,0));
        corners[2] = transform.TransformPoint(new Vector3(size.x / 2,  size.y / 2,0));
        corners[3] = transform.TransformPoint(new Vector3(-size.x / 2,  size.y / 2,0));

        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
    }
}
