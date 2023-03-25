using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Pathfinding;

public class Hit_Item : NetworkBehaviour
{
    //private float time = 0f;
    private GameManager_RPC gameManager_RPC;
    public LayerMask ItemLayer;
    private ulong target_girl_id;
    private ItemScript item;
    //public bool Activated_kill = false;
    public float HitBox_size = 0.75f;
    private bool ToggleBool = true;
    //[SerializeField] private AudioSource GhostKillEffect;
    SpawnManager spawnManager;
    // Start is called before the first frame update

    void Start()
    {
        gameManager_RPC = GameObject.FindWithTag("GameManager").GetComponent<GameManager_RPC>();
        spawnManager = GameObject.FindWithTag("SpawnManager").GetComponent<SpawnManager>();

        target_girl_id = 999;
    }

    

   


    [ServerRpc(RequireOwnership = false)]
    public void DisActivatedGhostServerRpc() {
        //ulong ghost_id = gameManager_RPC.ghost_id;
        //NetworkManager.SpawnManager.GetPlayerNetworkObject(ghost_id).GetComponent<SpriteRenderer>().enabled = false;
        DisActivatedGhostClientRpc();

    }

    [ClientRpc]
    public void DisActivatedGhostClientRpc() {
        //GameObject.FindWithTag("Wolf").GetComponent<Ghost_rule>().Activated_kill = false;
        GameObject.FindWithTag("Wolf").GetComponent<SpriteRenderer>().enabled = false;
        GameObject.FindWithTag("Wolf").GetComponent<Ghost_rule>().Activated_kill = false;

        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Ghost"))
        {
            gameObject.GetComponent<AIPath>().enabled = false;

        }
        StartCoroutine(WolfReset());
    }

    

    private IEnumerator WolfReset() {
        yield return new WaitForSeconds(2);
        GameObject.FindWithTag("Wolf").GetComponent<SpriteRenderer>().enabled = true;
        GameObject.FindWithTag("Wolf").GetComponent<Ghost_rule>().Activated_kill = true;

        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Ghost"))
        {
            //gameObject.GetComponent<AIDestinationSetter>().enabled = true;
            gameObject.GetComponent<AIPath>().enabled = true;

        }

    }

    [ServerRpc(RequireOwnership =false)]
    public void DespawnGhostBotServerRpc() {
        if(gameManager_RPC.DeadList.Count == 0) {
            Debug.Log("No ghost bot yet");
            return;
        }
        Debug.Log("Server_ItemA_Despawn");

        int index = 0;
        ulong botclient_id = gameManager_RPC.DeadList[index];
     
        NetworkManager.SpawnManager.GetPlayerNetworkObject(botclient_id).Despawn(true);
    }
    [ServerRpc(RequireOwnership = false)]
    public void SpawnGhostBotServerRpc(Vector3 Picked_Position)
    {
        Debug.Log("Server_ItemB_Spawn");
        int OffsetX   = Random.Range(-5,5);
        int OffsetY = Random.Range(-5, 5);
        Vector2 Random_position = new Vector2 (Picked_Position.x+ OffsetX ,Picked_Position.y+OffsetY);
        ulong ghost_id = gameManager_RPC.ghost_id;
        //spawnManager.SpawnBotServerRpc(ghost_id, 2, Random_position);
        spawnManager.SpawnBotServerRpc(NetworkManager.ServerClientId, 2, Random_position);
        
        
    }
   
}
