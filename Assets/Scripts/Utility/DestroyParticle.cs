using UnityEngine;

public class DestroyParticle : MonoBehaviour
{

    public float lifetime;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject,lifetime);
    }

}
