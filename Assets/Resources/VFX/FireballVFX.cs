
using UnityEngine;

public class FireballVFX : MonoBehaviour
{

    public Transform target;
    
    [SerializeField] float speed = 4f;
    private ParticleSystem _ps;
    private ParticleSystem.Particle[] _particles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _ps = GetComponentInChildren<ParticleSystem>();
        _particles = new ParticleSystem.Particle[_ps.main.maxParticles];
    }

    void Update()
    {
        MoveToTarget(target);
        
       
    }

    void MoveToTarget(Transform target)
    {
        if (target == null) return;

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        Vector3 dir = (target.position - transform.position).normalized;
        transform.forward = dir;

        float d = Vector3.Distance(transform.position, target.position);
        if (d < 0.1f)
        {
            Destroy(gameObject);
        }
    }
    
    
}
