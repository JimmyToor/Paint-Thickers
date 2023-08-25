using Src.Scripts.Gameplay;
using UnityEngine;

public class CollisionDamage : MonoBehaviour
{
    public float damage;
    public float hitCooldown;
    public TeamMember teamMember;
    
    private void OnCollisionEnter(Collision other)
    {
        if (TryGetComponent(out Health health))
        {
            if (teamMember != null
                && other.gameObject.TryGetComponent(out TeamMember team)
                && team.teamChannel == teamMember.teamChannel) return;
            
            health.TakeHit();
        }
    }
}
