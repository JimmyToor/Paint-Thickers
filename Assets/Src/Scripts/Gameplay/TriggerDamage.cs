using UnityEngine;

namespace Src.Scripts.Gameplay
{
    public class TriggerDamage : MonoBehaviour
    {
        public float damage;
        public float hitCooldownTime;
        public TeamMember teamMember;

        private float _currHitCooldown;

        private void Awake()
        {
            _currHitCooldown = hitCooldownTime;
        }

        private void Update()
        {
            _currHitCooldown -= Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            DoDamage(other);
        }

        private void OnTriggerStay(Collider other)
        {
            DoDamage(other);
        }

        private void DoDamage(Component target)
        {
            if (_currHitCooldown > 0 || !target.TryGetComponent(out Health health) || !IsValidTarget(target))
            {
                return;
            }
            
            health.TakeHit(damage*Time.deltaTime);
            _currHitCooldown = hitCooldownTime;
        }

        private bool IsValidTarget(Component target)
        {
            return teamMember != null
                    && target.gameObject.TryGetComponent(out TeamMember team)
                    && team.teamChannel == teamMember.teamChannel;
        }
    }
}
