namespace Volplane.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    using Volplane;

    public class Lobby : VolplaneBehaviour
    {
        // Variables

        // Main
        [SerializeField] protected bool lobbyActive = true;                 // Is lobby active
        [SerializeField] [Range(2, 6)] protected int minPlayerCount = 2;    // Min. amount of connected players to start a game
        [SerializeField] [Range(2, 6)] protected int maxPlayerCount = 6;    // Max. amount of players for this game

        // Following object variables are used as a visual representation
        // of the lobby
        [SerializeField] protected Player playerPrefab;                     // Player object prefab
        [SerializeField] protected Transform[] playerPositions;             // Player positions
        [SerializeField] protected Text[] playerNames;                      // Text object for player names

        protected List<Player> playerSlots;                                 // Player slot list

        // In this scene there are 6 'player slots' -> a maximum of six players can join.
        // 'playerPrefab'       is the player gameobject prefab
        // 'playerPositions'    specify the position where to instantiate the players (slot 0-6)
        // 'playerNames'        are the text objects used for the player names  (slot 0-6)

        /// <summary>
        /// Will start the game and exits the lobby if all players are ready.
        /// </summary>
        public void StartGame()
        {
            // Return when lobby inactive or not enough players
            if(!lobbyActive || (ActivePlayerCount < minPlayerCount))
                return;

            // Check if all connected players are ready by iterating through the player slots
            bool startGame = true;

            for(int i = 0; i < playerSlots.Count; i++)
            {
                if(playerSlots[i] != null)
                    startGame &= playerSlots[i].IsReady;
            }

            // All ready?
            if(startGame)
            {
                // Let's start the game....
                Debug.Log("Game start!");
                lobbyActive = false;
                ChangeViewAllActive("game");
            }
        }

        /// <summary>
        /// Adding player to lobby.
        /// </summary>
        /// <param name="playerId">Player identifier.</param>
        public void AddPlayer(int playerId)
        {
            if(!lobbyActive)
                return;

            // Is there room left for a new player?
            // 'ActivePlayerCount' will return the number of all connected players that are active.
            // Active players -> can send input
            // Inactive players -> are blocked
            // We will set all players in the lobby active.
            if(ActivePlayerCount <= maxPlayerCount)
            {
                // Find a free slot
                int slotIndex = GetFreeSlot();

                // Instantiate a new player object in the scene
                Player playerObj = Instantiate<Player>(playerPrefab, playerPositions[slotIndex].position, Quaternion.identity);

                // Set the player id to the instantiated player object
                playerObj.playerId = playerId;

                // Set a reference to the text object from this slot by calling players 'PlayerText' property
                playerObj.PlayerText = playerNames[slotIndex];

                // Save player in a free slot
                playerSlots[slotIndex] = playerObj;

                // Is this new player the game master?
                if(playerId == GetMasterId())
                    SetMaster(playerId);

                SetActive(playerId, true);      // Set this device active
                ResetView(playerId, "lobby");   // Reset the lobby view (and its elements) to its initial state
                ChangeView(playerId, "lobby");  // Change to the lobby view
            }
            else
            {
                // Spectating
                SetActive(playerId, false);     // Set this player inactive
                ChangeView(playerId, "full");   // Game / lobby is full -> change the controller view to inform the player
            }
        }

        /// <summary>
        /// Kicks a player from the lobby.
        /// </summary>
        /// <param name="playerId">Player identifier.</param>
        public void KickPlayer(int playerId)
        {
            if(!lobbyActive)
                return;

            // Check if specified player is in the lobby by iterating through the player slots
            int slotIndex = -1;

            for(int i = 0; i < playerSlots.Count; i++)
            {
                if(playerSlots[i].playerId == playerId)
                {
                    slotIndex = i;
                    i = playerSlots.Count;
                }
            }

            // Return if there is no player with this identifier in the lobby
            if((slotIndex == -1) || (playerSlots[slotIndex] == null))
                return;

            // Destroy this player object on the scene and reassign this lsot with 'null'
            Destroy(playerSlots[slotIndex].gameObject);
            playerSlots[slotIndex] = null;

            // Set player inactive
            SetActive(playerId, false);

            // Are there still more players connected than in the lobby?
            // This would mean that there are some spectators who could now join the game.
            if(PlayerCount >= maxPlayerCount)
            {
                // Holder variable for waiting players identifier
                int waitingPlayerId;

                // Check if there is a player waiting who is also the game master.
                // This is very unlikely, but it can happen -> for example when one or multiple user buy AirConsole Hero.
                // Heros are likelier to become game master.
                // If master is active -> it is already in the lobby
                if(GetMaster().IsActive)
                    waitingPlayerId = GetAllInactivePlayers().First(x => x.IsConnected).PlayerId;   // Search for a yet inactive player to join the lobby
                else
                    waitingPlayerId = GetMasterId();                                                // Select game master to join the lobby

                // Instantiate a new player object in the scene
                playerSlots[slotIndex] = Instantiate<Player>(playerPrefab, playerPositions[slotIndex].position, Quaternion.identity);

                // Set the player id to the instantiated player object
                playerSlots[slotIndex].playerId = waitingPlayerId;

                // Set a reference to the text object from this slot by calling players 'PlayerText' property
                playerSlots[slotIndex].PlayerText = playerNames[slotIndex];

                // Move player to lobby
                SetActive(waitingPlayerId, true);
                ResetView(waitingPlayerId, "lobby");
                ChangeView(waitingPlayerId, "lobby");
                ChangeView(playerId, "full");
            }
            else
            {
                // No waiting players...

                // Disable text object of the removed player from this slot
                playerNames[slotIndex].gameObject.SetActive(false);

                // Change controller view back to 'waiting' view
                ChangeView(playerId, "waiting");
            }

            // Call set master for the current master controller applying any master transfers onto the player objects
            SetMaster(GetMasterId());
        }

        /// <summary>
        /// Searches for a free player slot.
        /// </summary>
        /// <returns>The index of the free slot.</returns>
        protected int GetFreeSlot()
        {
            for(int i = 0; i < playerPositions.Length; i++)
            {
                if(playerSlots[i] == null)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Sets the crown for the given player object and disables it for other players.
        /// </summary>
        /// <param name="playerId">Player identifier.</param>
        protected void SetMaster(int playerId)
        {
            if(!lobbyActive)
                return;

            // Iterate through all slots
            for(int i = 0; i < playerSlots.Count; i++)
            {
                if(playerSlots[i] == null)
                    continue;

                // Set the crown when correct player is found
                if(playerSlots[i].playerId == playerId)
                    playerSlots[i].SetMaster(true);
                else
                    playerSlots[i].SetMaster(false);
            }
        }

        /// <summary>
        /// 'MonoBehaviour.Start()' method from Unity
        /// Start is called on the frame when a script is enabled just before any of the Update methods
        /// is called the first time.
        /// </summary>
        private void Start()
        {
            // Initialize player slots (6)
            playerSlots = new List<Player>(6) { null, null, null, null, null, null };

            // Set standard view for joining players
            SetStandardView("waiting");

            if(playerPositions.Length > 6)
                Debug.LogError("[Lobby] Cannot assign more than 6 player positions.");

            if(playerNames.Length > 6)
                Debug.LogError("[Lobby] Cannot assign more than 6 player names (text objects).");
        }

        /// <summary>
        /// 'Volplane.OnConnect()' method from the Volplane framework
        /// OnConnect is called when a new AirConsole player joins the session.
        /// </summary>
        /// <param name="player">The player object of the connected device.</param>
        private void OnConnect(int playerId)
        {
            if(!lobbyActive)
                return;

            // Try to add this player to the lobby
            AddPlayer(playerId);
        }

        /// <summary>
        /// 'Volplane.OnDisconnect()' method from the Volplane framework
        /// OnDisconnect is called when an AirConsole player left the session.
        /// </summary>
        /// <param name="player">The player object of the disconnected device.</param>
        private void OnDisconnect(int playerId)
        {
            if(!lobbyActive)
                return;

            // Kick disconnected player
            KickPlayer(playerId);
        }

        /// <summary>
        /// 'Volplane.OnHero()' method from the Volplane framework
        /// OnHero is called when a player becomes AirConsole Hero or an AirConsole Hero player connects.
        /// </summary>
        /// <param name="player">The player object of the AirConsole Hero device.</param>
        private void OnHero(VPlayer player)
        {
            if(!lobbyActive)
                return;
            
            // It could be that the game master has now changed
            if(GetMasterId() == player.PlayerId)
            {
                // If this player is the new master and it is not active (just spectating)
                // kick an active player and take its slot instead!
                if(!player.IsActive)
                    KickPlayer(GetAllActivePlayers().Last().PlayerId);
            }
        }
	}
}
