using UnityEngine;
using UnityEngine.UIElements;

public class TreeTuner : MonoBehaviour
{
    public Vector2 heightScale = new Vector2(60, 90);
    public Vector2 widthScale = new Vector2(70, 85);
    public int personnalityRange = 10;
    public float rotationRange =  5;
    public float maxPositionOffset = 0.05f;
    void OnEnable()
    {
        MakeUnique();
    }

    void MakeUnique()
    {
        float baseWidthScale = Random.Range(widthScale.x, widthScale.y);
        float baseHeightScale = Random.Range(heightScale.x, heightScale.y);
        Vector3 personnality = new Vector3(Random.Range(-personnalityRange, personnalityRange), Random.Range(-personnalityRange, personnalityRange), Random.Range(-personnalityRange, personnalityRange));
        transform.localScale = personnality + new Vector3(baseWidthScale, baseHeightScale, baseWidthScale);
        float x_rotation = Random.Range(-rotationRange, rotationRange);
        float z_rotation = Random.Range(-rotationRange, rotationRange);
        float y_rotation = Random.Range(0, 360f);
        transform.rotation = Quaternion.Euler(-90 + x_rotation, y_rotation, z_rotation);
        float x_offset = Random.Range(-maxPositionOffset, maxPositionOffset);
        float z_offset = Random.Range(-maxPositionOffset, maxPositionOffset);
        transform.position = transform.position + new Vector3(x_offset, 0, z_offset);
    }
}
