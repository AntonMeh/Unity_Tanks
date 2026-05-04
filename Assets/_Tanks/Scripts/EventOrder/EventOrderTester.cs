using UnityEngine;

public class EventOrderTester : MonoBehaviour
{
    void Awake()
    {
        Debug.Log($"{name}: Awake");
    }

    void OnEnable()
    {
        Debug.Log($"{name}: OnEnable");

    }

    void Start()
    {
        Debug.Log($"{name}: Start");
    }

    void Update()
    {
        Debug.Log($"{name}: Update");
        enabled = false;
    }

    void OnDisable()
    {
        Debug.Log($"{name}: OnDisable");
    }

    void OnDestroy()
    {
        Debug.Log($"{name}: OnDestroy");
    }
}
