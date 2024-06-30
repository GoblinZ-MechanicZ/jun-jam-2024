using UnityEngine;

public class BolderRotate : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 10f;
    void Update()
    {
        transform.Rotate(_rotationSpeed * Time.deltaTime * Vector3.right);
    }
}
