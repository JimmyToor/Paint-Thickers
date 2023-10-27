using System;
using UnityEngine;

namespace Src.Scripts.ScriptableObjects
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Weapon Parameters", menuName = "Data Objects/Weapon Parameters Object")]
    public class WeaponParameters : ScriptableObject
    {
        public float maxAmmo;
        [Tooltip("How much ammo to consume when starting to fire.")]
        public float initialUsage;
        public float refillRate;
        public float usageRate;
        [Tooltip("Ammo refills automatically once below this threshold.")]
        public float lowRefillThreshold;
        [Tooltip("Seconds to wait after falling below ammo threshold to refill ammo.")]
        public float lowAmmoRegenCooldownTime;
        public GameObject weaponPrefab;
    }
}