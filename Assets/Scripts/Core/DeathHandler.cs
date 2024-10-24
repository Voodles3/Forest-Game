using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Forest.Core
{
    public class DeathHandler : MonoBehaviour
    {
        [SerializeField] Vector3 respawnCoords = new();

        public void Die()
        {
            transform.position = respawnCoords;
        }
    }
}