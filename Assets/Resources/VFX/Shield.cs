using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shield : MonoBehaviour
{
    [SerializeField] float dissolveSpeed = 0.6f;
    [SerializeField] float waveAmplitude = 1f;
    [SerializeField] float pulseSpeed = 3f;
    private Vector3 _baseScale;
    private Renderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        StartCoroutine(DeactivateShield());
        _baseScale = transform.localScale;
    }

    private void Update()
    {
        float scale = 1 + Mathf.Sin(Time.time * pulseSpeed) * waveAmplitude;
        transform.localScale = _baseScale * scale;
    }

    public IEnumerator DeactivateShield()
    {
        _renderer.material.SetFloat("_DissolveFactor", 0f);
        float start = 0f;
        float target = 1.1f;
        float lerp = 0f;
        while(lerp < 1)
        {
            _renderer.material.SetFloat("_DissolveFactor", Mathf.Lerp(start, target, lerp));
            lerp += Time.deltaTime * dissolveSpeed;
            yield return null;
        }
        _renderer.material.SetFloat("_DissolveFactor", target);
        yield return new WaitForSeconds(3f);
        StartCoroutine(ActivateShield());
    }

    public IEnumerator ActivateShield()
    {
        _renderer.material.SetFloat("_DissolveFactor", 1f);
        float start = 1f;
        float target = 0f;
        float lerp = 0f;
        while (lerp < 1)
        {
            _renderer.material.SetFloat("_DissolveFactor", Mathf.Lerp(start, target, lerp));
            lerp += Time.deltaTime * dissolveSpeed;
            yield return null;
        }
        _renderer.material.SetFloat("_DissolveFactor", target);
    }
}
