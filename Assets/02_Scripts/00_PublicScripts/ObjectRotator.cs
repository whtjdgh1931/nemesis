using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [SerializeField] float _rotationPeriod;
    [SerializeField] Vector3 _axis = Vector3.up;

    void Update()
    {
        if (_rotationPeriod <= 0f) return; // 안전장치

        float degreesPerSecond = 360f / _rotationPeriod;
        transform.Rotate(_axis, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}