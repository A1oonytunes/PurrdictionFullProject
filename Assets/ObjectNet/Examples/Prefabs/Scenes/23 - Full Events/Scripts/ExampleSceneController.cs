using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet.examples {

    /// <summary>
    /// 
    /// This scene show how to implements all logic on you own script without using the tools provided by ObjectNet
    /// 
    /// You can create you own managers, events and everthing using only ObjectNet transport, events and delivery system provided by ObjectNet
    /// 
    /// </summary>
    public class ExampleSceneController : MonoBehaviour {

        [SerializeField]
        private GameObject playerPrefab;

        [SerializeField]
        private Transform positionToSpawn;

        [SerializeField]
        private Camera mainCamera;

        // Store player ID and player object
        private Dictionary<int, Tuple<IClient, GameObject>> currentPlayers = new Dictionary<int, Tuple<IClient, GameObject>>();

        int currentIdFactory = 1;

        int playerOwnerId = 0;

        float nextUpdatePosition = 0f;

        float nextUpdateRotation = 0f;

        // Game Events
        const int SPAWN_PLAYER_EVENT        = 20001;
        const int PLAYER_POSITION_EVENT     = 20002;
        const int PLAYER_ROTATION_EVENT     = 20003;

        // Update constants
        const float POSITION_UPDATE_RATE    = (1f / 30f);
        const float POSITION_ROTATION_RATE  = (1f / 10f);

        private void Start() {
            NetworkManager.Events.RegisterEvent(SPAWN_PLAYER_EVENT,     this.OnPlayerSpawnReceived);
            NetworkManager.Events.RegisterEvent(PLAYER_POSITION_EVENT,  this.OnReceivePosition);
            NetworkManager.Events.RegisterEvent(PLAYER_ROTATION_EVENT,  this.OnReceiveRotation);
        }

        private void Update() {
            if (this.nextUpdatePosition < Time.time) {
                foreach (var playerEntry in this.currentPlayers) {
                    // Send player position update ( 30 updates peer second )
                    if (this.nextUpdatePosition < Time.time) {
                        using (DataStream writer = new DataStream()) {
                            writer.Write(playerEntry.Key);
                            writer.Write(playerEntry.Value.Item2.transform.position);
                            if (NetworkManager.Instance().IsServerConnection()) {
                                foreach (var sendEntry in this.currentPlayers) {
                                    if (playerEntry.Value.Item1 != sendEntry.Value.Item1) {
                                        if (sendEntry.Value.Item1 != null) {
                                            sendEntry.Value.Item1.Send(PLAYER_POSITION_EVENT, writer, DeliveryMode.Unreliable);
                                        }
                                    }
                                }
                            } else if (this.playerOwnerId == playerEntry.Key) {
                                playerEntry.Value.Item1.Send(PLAYER_POSITION_EVENT, writer, DeliveryMode.Unreliable);
                                break;
                            }
                        }
                    }
                }
                this.nextUpdatePosition = (Time.time + POSITION_UPDATE_RATE);
            }
            if (this.nextUpdateRotation < Time.time) {
                foreach (var playerEntry in this.currentPlayers) {
                    // Send player rotation update ( 10 updates peer second )
                    if (this.nextUpdateRotation < Time.time) {
                        using (DataStream writer = new DataStream()) {
                            writer.Write(playerEntry.Key);
                            writer.Write(playerEntry.Value.Item2.transform.rotation.eulerAngles);
                            if (NetworkManager.Instance().IsServerConnection()) {
                                foreach (var sendEntry in this.currentPlayers) {
                                    if (playerEntry.Value.Item1 != sendEntry.Value.Item1) {
                                        if (sendEntry.Value.Item1 != null) {
                                            sendEntry.Value.Item1.Send(PLAYER_ROTATION_EVENT, writer, DeliveryMode.Unreliable);
                                        }
                                    }
                                }
                            } else if (this.playerOwnerId == playerEntry.Key) {
                                playerEntry.Value.Item1.Send(PLAYER_ROTATION_EVENT, writer, DeliveryMode.Unreliable);
                                break;
                            }
                        }
                    }
                }
                this.nextUpdateRotation = (Time.time + POSITION_ROTATION_RATE);
            }
        }

        public void OnServerStarted(IChannel channel) {
            GameObject createdObject = GameObject.Instantiate(this.playerPrefab, this.positionToSpawn.position, Quaternion.identity);
            this.currentPlayers.Add(this.currentIdFactory++, new Tuple<IClient, GameObject>(null, createdObject));
            this.mainCamera.gameObject.SetActive(false);
        }

        public void OnClientConnected(IClient client) {
            GameObject createdObject = GameObject.Instantiate(this.playerPrefab, this.positionToSpawn.position, Quaternion.identity);
            createdObject.GetComponent<PlayerControllerExample>().enabled = false; // Disable on client's
            createdObject.transform.Find("PlayerCamera").gameObject.SetActive(false);
            this.currentPlayers.Add(this.currentIdFactory++, new Tuple<IClient, GameObject>(client, createdObject));
            // notify client regard to this nw player
            foreach(var playerEntry in this.currentPlayers) {
                using (DataStream writer = new DataStream()) {
                    writer.Write(playerEntry.Key);
                    writer.Write(playerEntry.Value.Item2.transform.position);
                    writer.Write(playerEntry.Value.Item2 == createdObject); // Is this the new player ( i need to tell is will be active or passive )
                    client.Send(SPAWN_PLAYER_EVENT, writer, DeliveryMode.Reliable);
                }                
            }
        }

        void OnPlayerSpawnReceived(IDataStream reader) {
            int     receivedId          = reader.Read<int>();
            Vector3 receivedPosition    = reader.Read<Vector3>();
            bool    isOwner             = reader.Read<bool>();
            GameObject createdObject    = GameObject.Instantiate(this.playerPrefab, receivedPosition, Quaternion.identity);
            this.currentPlayers.Add(receivedId, new Tuple<IClient, GameObject>((reader as INetworkStream).GetClient(), createdObject));
            // If isn't owner i need to disable movement component
            if (!isOwner) {
                createdObject.GetComponent<PlayerControllerExample>().enabled = false;
                createdObject.transform.Find("PlayerCamera").gameObject.SetActive(false);
            } else {
                this.playerOwnerId = receivedId;
                this.mainCamera.gameObject.SetActive(false);
            }
        }

        void OnReceivePosition(IDataStream reader) {
            int     playerId        = reader.Read<int>();
            Vector3 playerPosition  = reader.Read<Vector3>();
            // Update position
            if (this.currentPlayers.ContainsKey(playerId)) {
                this.currentPlayers[playerId].Item2.transform.position = playerPosition;
            }
        }
        void OnReceiveRotation(IDataStream reader) {
            int     playerId        = reader.Read<int>();
            Vector3 playerEuler     = reader.Read<Vector3>();
            // Update position
            if (this.currentPlayers.ContainsKey(playerId)) {
                this.currentPlayers[playerId].Item2.transform.rotation = Quaternion.Euler(playerEuler);
            }
        }
        
    }
}