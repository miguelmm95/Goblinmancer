using UnityEngine;
using UnityEngine.VFX;

public class PentagramVFX : MonoBehaviour
{

    public VisualEffect pentagramVFX;
    public VisualEffect auraVFX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(pentagramVFX.aliveParticleCount == 0)
        {
            auraVFX.Stop();
            Destroy(gameObject);
        }
    }
}
