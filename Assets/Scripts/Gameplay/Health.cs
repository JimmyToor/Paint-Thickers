using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int hitpoints;
    public bool invulnerable;
    public UnityEvent onDeath;
    
    public void ReduceHP(int damage)
    {
        if (!invulnerable)
        {
            hitpoints -= damage;
            if (hitpoints <= 0)
            {
                onDeath.Invoke();
            }
        }
    }
}
