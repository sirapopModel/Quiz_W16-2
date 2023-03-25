using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Unity.Netcode;
using UnityEngine;

public class Girl_Rule : NetworkBehaviour
{
    private GameManager_RPC gameManager_RPC;
    public Transform Target_Transform;
    public bool Set_Transform = false;
    public bool IsServerOwn = false;
//    private bool Started_hunt = false;
    

    public override void OnNetworkSpawn()
    {
        gameManager_RPC = GameObject.FindWithTag("GameManager").GetComponent<GameManager_RPC>();
        ulong client_id = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().OwnerClientId;
        request_target_transformServerRpc(client_id,IsServerOwn);
    }

    



    [ServerRpc(RequireOwnership = false)]
    private void request_target_transformServerRpc(ulong client_id,bool IsServerOwner)
    {
        if (gameManager_RPC.HuntingList.Count == 0)
        {
            return;
        }
        //Debug.Log("Bot has get transform target");
        //Debug.Log($"ghost : {client_id}");
        int target_index = Random.Range(0, gameManager_RPC.HuntingList.Count);
        ulong target_id = gameManager_RPC.HuntingList[target_index];
        Debug.Log($"Bot has get target id:{target_id}");
       
        GameObject target_Object = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(target_id).gameObject;
        Transform target_transform = target_Object.transform;
        if (IsServerOwner == true)
        {
            gameObject.GetComponent<AIDestinationSetter>().target = target_transform;
            return;
        }
        NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(client_id).gameObject.GetComponent<AIDestinationSetter>().target = target_transform;

        //Set_TransformBool_ClientRpc(client_id, target_position);
    }


    //[ClientRpc]
    //private void Set_TransformBool_ClientRpc(ulong client_id,Vector3 target_position) {
    //    if(NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().OwnerClientId == client_id) {
    //        Debug.Log($"ghost Servant : {NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().OwnerClientId}");
    //        //Debug.Log($"AI Target: {target_id}");
    //        NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<Girl_Rule>().Started_hunt = true;
    //    }
    //}


}
