using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class LightFlicker2D : MonoBehaviour
{
    [SerializeField] private float speed = 4f;
    [SerializeField] private float amount = 0.2f;
    [SerializeField] private float baseIntensity = 1.2f;

    private Light2D _light;
    private float _seed;

    private void Awake()
    {
        _light = GetComponent<Light2D>();
        if (_light != null && baseIntensity <= 0f)
            baseIntensity = _light.intensity;

        _seed = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (_light == null) return;
        var noise = Mathf.PerlinNoise(Time.time * speed + _seed, _seed);
        _light.intensity = baseIntensity + (noise - 0.5f) * amount;
    }
}
