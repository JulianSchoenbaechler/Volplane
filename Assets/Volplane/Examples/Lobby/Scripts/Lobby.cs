namespace Volplane.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Volplane;

    public class Lobby : VolplaneBehaviour
    {
        [Range(2, 6)] public int minPlayerCount = 2;
        [Range(2, 6)] public int maxPlayerCount = 6;
        public Player playerPrefab;
        public Transform[] playerPositions;

        private List<int> playerSlots;

        private void Start()
        {
            playerSlots = new List<int>(6) { -1, -1, -1, -1, -1, -1 };

            SetStandardView("waiting");

            if(playerPositions.Length > 6)
                Debug.LogError("[Lobby] Cannot assign more than 6 player positions.");
        }

        private void OnConnect(int playerId)
        {
            if(PlayerCount <= 6)
            {
                Player playerObj = Instantiate<Player>(playerPrefab, GetFreeSlot(playerId), Quaternion.identity);
                playerObj.playerId = playerId;

                if(playerId == GetMasterId())
                    playerObj.SetMaster(true);

                SetActive(playerId, true);
                ChangeView(playerId, "lobby");
            }
            else
            {
                // Spectating
                SetActive(playerId, false);
                ChangeView(playerId, "full");
            }
        }

        private void OnDisconnect(int playerId)
        {
            Player playerObj = GameObject.FindObjectsOfType<Player>().FirstOrDefault(x => x.playerId == playerId);

            if(playerObj == null)
                return;
            
            Destroy(playerObj.gameObject);

            SetFreeSlot(playerId);

            if(playerId == GetMasterId())
            {
                // Master left
            }
            else if(PlayerCount >= 6)
            {
                int waitingPlayerId = GetAllInactivePlayers().First(x => x.IsConnected).PlayerId;

                playerObj = Instantiate<Player>(playerPrefab, GetFreeSlot(waitingPlayerId), Quaternion.identity);
                playerObj.playerId = waitingPlayerId;

                // Move player to lobby
                SetActive(playerId, true);
                ChangeView(waitingPlayerId, "lobby");
            }
        }

        private void OnHero(int playerId)
        {
            // It could be that the game master changed...
            foreach(Player player in GameObject.FindObjectsOfType<Player>())
            {
                if(player.playerId == GetMasterId())
                    player.SetMaster(true);
                else
                    player.SetMaster(false);
            }
        }

        private Vector3 GetFreeSlot(int playerId)
        {
            for(int i = 0; i < playerPositions.Length; i++)
            {
                if(playerSlots[i] == -1)
                {
                    playerSlots[i] = playerId;
                    return playerPositions[i].position;
                }
            }

            return Vector3.zero;
        }

        private void SetFreeSlot(int playerId)
        {
            for(int i = 0; i < playerPositions.Length; i++)
            {
                if(playerSlots[i] == playerId)
                {
                    playerSlots[i] = -1;
                    return;
                }
            }
        }



        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                print(VolplaneController.AirConsole.GetMasterControllerDeviceId());
                print(GetMasterId());
            }
        }

	}
}
