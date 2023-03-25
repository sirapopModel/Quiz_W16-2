using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

public class SpawnManager : NetworkBehaviour
{

    //[SerializeField] private GameObject playerPrefabA; //add prefab in inspector
    //[SerializeField] private GameObject playerPrefabB; //add prefab in inspector

    //private bool Activatespawn = true;
    

    [SerializeField] private List<GameObject> Network_Prefab_List;
    [SerializeField] private List<GameObject> HumanPrefabList;
    public Dictionary<ulong,int> ClientHumanIndex = new Dictionary<ulong, int>();

    private GameManager_RPC gameManagerRPC; //using only server side 
    //public LobbyManager lobby;

    ////when new connected is successful the network spawn be executed
    public override void OnNetworkSpawn()
    {
        //success Connected
        gameManagerRPC = GameObject.FindWithTag("GameManager").GetComponent<GameManager_RPC>();
        //lobby = GameObject.FindWithTag("Canvas").GetComponentInChildren<LobbyManager>();
    }

    private int GetAvailableHumanPrefabIndex()
    {
        int Prefabindex = 0;
        while (ClientHumanIndex.ContainsValue(Prefabindex))
        {
            Prefabindex++;
        }
        return Prefabindex;
    }

    //Spawn Game Player Object with Prefab index that setting by LobbyManager
    [ServerRpc(RequireOwnership = false)]//server owns this object but client can request a spawn
    public void SpawnPlayerPrefabServerRpc(ulong client_id, int Prefabindex ,Vector2 position, ServerRpcParams serverRpcParams = default)
    {
        GameObject gameObject = (GameObject)Instantiate(Network_Prefab_List[Prefabindex]); // note : need to use a clone from prefab by using Instantitiate!!
        gameObject.SetActive(true);
        gameObject.transform.position = position;
        gameObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(client_id); // Beware!! this one have latency
        
        Debug.Log($"FROM SpawnManager : ServerClientID{client_id}: spawned!");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBotServerRpc(ulong client_id, int Prefabindex, Vector2 position) {
        GameObject gameObject = (GameObject)Instantiate(Network_Prefab_List[Prefabindex]); // note : need to use a clone from prefab by using Instantitiate!!
        gameObject.SetActive(true);
        gameObject.transform.position = position;
        if(Prefabindex == 2) {
            gameObject.GetComponent<Girl_Rule>().IsServerOwn = true;
        }
        gameObject.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.ServerClientId);
        //gameObject.GetComponent<NetworkObject>().Spawn(true);

    }

    // ----For Human----
    //Spawn Game Player Object with Prefab index that setting by LobbyManager
    [ServerRpc(RequireOwnership = false)]//server owns this object but client can request a spawn
    public void SpawnHumanPrefabServerRpc(ulong client_id, Vector2 position, ServerRpcParams serverRpcParams = default)
    {
        // If the client already have a human
        int Prefabindex;
        if (ClientHumanIndex.ContainsKey(client_id))
        {
            Prefabindex = ClientHumanIndex[client_id];
        }
        else
        {
            // If not, Get available human prefab index
            Prefabindex = GetAvailableHumanPrefabIndex();
            ClientHumanIndex[client_id] = Prefabindex;
        }

        GameObject gameObject = (GameObject)Instantiate(HumanPrefabList[Prefabindex]); // note : need to use a clone from prefab by using Instantitiate!!
        gameObject.SetActive(true);
        gameObject.transform.position = position;
        gameObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(client_id); // Beware!! this one have latency

        Debug.Log($"FROM SpawnManager : ServerClientID{client_id}: spawn as {HumanPrefabList[Prefabindex].name}!");
    }

    //Despawn All Client Object
    [ServerRpc]
    public void DespawnAllclientServerRpc()
    {
        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Player"))
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
        }
        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Ghost"))
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
        }
        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Wolf"))
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
        }
        //foreach (NetworkClient Client_obj in NetworkManager.Singleton.ConnectedClientsList)
        //{
        //    if (Client_obj.PlayerObject != null)
        //    {
        //        Client_obj.PlayerObject.Despawn();

        //    }
        //}
}

    [ServerRpc(RequireOwnership = false)]
    public void DespawnPlayerServerRpc(ulong target_id, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"Despawn Player: {target_id}!!");
        NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(target_id).Despawn();
    }

}
