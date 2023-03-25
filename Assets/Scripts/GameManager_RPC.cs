using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;
using UnityEngine.Events;

public class GameManager_RPC : NetworkBehaviour
{
    public List<ulong> HuntingList;
    private bool IsGhostWin = false;
    public float EndGameTime;
    private float time = 0f;
    public bool IsgameStart = false;
    public ulong ghost_id = 99 ; //Assign by SpawnManager Who is ghost,but for now host is ghost!
    public LobbyManager lobby;
    public List<ulong> DeadList;
    public List<ulong> BotList;
    private SpawnManager spawnManager;

    public UnityEvent OnGameEnd;

    //Start Project do something
    public override void OnNetworkSpawn()
    {
        HuntingList = new List<ulong>();
        lobby = GameObject.FindWithTag("Canvas").GetComponentInChildren<LobbyManager>();
        spawnManager = GameObject.FindWithTag("SpawnManager").GetComponentInChildren<SpawnManager>();
    }



    //Update a game status per frame 
    public void Update()
    {
        if ( (IsHost) && (HuntingList.Count ==0 ) && IsgameStart )
        {
            IsGhostWin = false; // Issue solve by R jarn Sopon's Advice
            IsgameStart = false;
            time = 0;
            WinnerFoundClientRpc(1,"Winner is Ghost!");
            OnGameEnd.Invoke();
        }
    }
    
    //Game time counter
    public void FixedUpdate()
    {
        if ( (IsHost) && IsgameStart && !IsGhostWin && (time < EndGameTime) )
        {
            time += Time.deltaTime;
        }

        if ((IsHost) && time > EndGameTime && !IsGhostWin && IsgameStart)
        {
            IsGhostWin = false;
            IsgameStart = false;
            time = 0;
            WinnerFoundClientRpc(0,"Winner is Humanity");
            OnGameEnd.Invoke();
        }
    }


    ////Announce All Player the winner is 
    //[ServerRpc(RequireOwnership = false)]
    //public void PlayerAnnounceWinServerRpc(bool IsghostWin) {
    //    if (!IsghostWin) {
    //        WinnerFoundClientRpc("Winner is Humanity!");
    //        return;
    //    }
    //    WinnerFoundClientRpc("Winner is Ghost!");
    //}

    //Add human player id to deadlist
    public void DeadListAdd(ulong NewGirl_id)
    {
        HuntingList.Add(NewGirl_id);
        Debug.Log($"FROM GameManager , ID: {NewGirl_id} has been added to DeadList. DeadList.");
    }

    //Server Despawn Girl who hit a ghost!!
    [ServerRpc(RequireOwnership =false)]
    public void GhostKillGirlServerRpc(ulong target_id,ServerRpcParams serverRpcParams = default)
    {
        //Vector3 dead_position = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(target_id).transform.position;

        Debug.Log($"Despawn Player: {target_id}!!");
        spawnManager.DespawnPlayerServerRpc(target_id);//Despawn Human Prefab
        StartCoroutine(SpawnGhostBot(target_id, new Vector3(11.47f, -19.39f, 0f)));
        DeadList.Add(target_id);
        IsGhostWinClientRpc(target_id);
    }

    public IEnumerator SpawnGhostBot(ulong owner_id , Vector3 position) {
        yield return new WaitForSeconds(2);
        spawnManager.SpawnPlayerPrefabServerRpc(owner_id, 2, position);
    }

    //ghost Confirm Kill call by ghost players!!
    [ClientRpc]
    private void  IsGhostWinClientRpc(ulong target_id)
    {
        HuntingList.Remove(target_id);
        if(HuntingList.Count == 0)
        {
            IsGhostWin = true;
        }

        Debug.Log($"removed : {target_id} from DeadList!");
        Debug.Log($"From DeadList:");
        foreach (ulong id in HuntingList)
        {
            Debug.Log($"={id}");
        }
    }

    // Start a Counter through IsgameStart
    [ClientRpc]
    public void CountDownClientRpc() { 
            IsgameStart = true;
    }


    //Response To all Client that's not a host
    [ClientRpc]
    private void WinnerFoundClientRpc(int winner_index , string message) {
        time = 0;
        IsgameStart = false;
        IsGhostWin = false;
        Debug.Log($"Message from client id {NetworkManager.LocalClientId}: {message}");
        lobby.ShowWinner(winner_index);

    }

    //Server allow ghost kill human player
    [ClientRpc]
    private void ActivatedGhostClientRpc(ulong clientID) {
        NetworkObject local_player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (clientID == local_player.OwnerClientId) {
            local_player.GetComponent<Ghost_rule>().Activated_kill = true;
            Debug.Log("Ghost Activated Kill");
        }
    }

   

   
}
