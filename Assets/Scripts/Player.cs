using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int teamChannel = 0; // colour channel of the player's team
    private void Start() {
        GetComponent<CharacterController>().tag = "Player";
    }
    
}
