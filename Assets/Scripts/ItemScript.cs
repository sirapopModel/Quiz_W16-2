using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
//using static UnityEditor.Progress;

public class ItemScript : NetworkBehaviour
{
    public string type;
    public bool Isused = false;
    private GameObject player;
    private int ItemB_GhostNum = 3 ;
    // Start is called before the first frame update
   
    
    

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (this.Isused)
        {
            return;
        }

        player = collision.gameObject; // get collision from other Object entered
        
        if (player.CompareTag("Player") && !this.Isused) {
            HitItem playerHit = collision.gameObject.GetComponent<HitItem>();

            if (type == "A")
            {
                Debug.Log("Item A used");
                playerHit.DespawnGhostBotServerRpc();
            }
            else if (type == "B")
            {
                for(int count = 0; count < ItemB_GhostNum; count++) {
                    playerHit.SpawnGhostBotServerRpc(gameObject.transform.position);
                }
            }
            else 
            {
                playerHit.DisActivatedGhostServerRpc();
            }
            DespawnItemServerRpc();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void DespawnItemServerRpc() {
        Used_alreadyClientRpc();
        NetworkObject.Despawn();
    }

    [ClientRpc]
    public void Used_alreadyClientRpc() {
        this.Isused = true;
    }




   
}

