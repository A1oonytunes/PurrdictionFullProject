using System.Collections;
using PurrLobby;
using PurrNet;
using PurrNet.Logging;
using PurrNet.Steam;
using PurrNet.Transports;
using Steamworks;
using UnityEngine;

public class ConnectionStarter : MonoBehaviour
{
        private PurrTransport _purrTransport;
        private SteamTransport _steamTransport;
        private UDPTransport _udpTransport;
        private NetworkManager _networkManager;
        private LobbyDataHolder _lobbyDataHolder;

        private bool _isFromLobby;
        
        private void Awake()
        {
            if(!TryGetComponent(out _steamTransport)) {
                PurrLogger.LogError($"Failed to get {nameof(SteamTransport)} component.", this);
            }
            if(!TryGetComponent(out _purrTransport)) {
                PurrLogger.LogError($"Failed to get {nameof(PurrTransport)} component.", this);
            }
            if(!TryGetComponent(out _udpTransport)) {
                PurrLogger.LogError($"Failed to get {nameof(UDPTransport)} component.", this);
            }
            if(!TryGetComponent(out _networkManager)) {
                PurrLogger.LogError($"Failed to get {nameof(NetworkManager)} component.", this);
            }
            
            _lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();
            if (_lobbyDataHolder)
                _isFromLobby = true;
        }

        private void Start()
        {
            if (!_networkManager)
            {
                PurrLogger.LogError($"Failed to start connection. {nameof(NetworkManager)} is null!", this);
                return;
            }
            if (!_steamTransport)
            {
                PurrLogger.LogError($"Failed to start connection. {nameof(SteamTransport)} is null!", this);
                return;
            }
            if (!_purrTransport)
            {
                PurrLogger.LogError($"Failed to start connection. {nameof(PurrTransport)} is null!", this);
                return;
            }
            
            if(_isFromLobby)
                StartFromLobby();
            else
                StartNormal();
            
            
#if UTP_LOBBYRELAY
            else if(_networkManager.transport is UTPTransport) {
                if(_lobbyDataHolder.CurrentLobby.IsOwner) {
                    (_networkManager.transport as UTPTransport).InitializeRelayServer((Allocation)_lobbyDataHolder.CurrentLobby.ServerObject);
                }
                (_networkManager.transport as UTPTransport).InitializeRelayClient(_lobbyDataHolder.CurrentLobby.Properties["JoinCode"]);
            }
#else
                //P2P Connection, receive IP/Port from server
#endif

         
        }
            private void StartNormal()
            {
                _networkManager.transport = _udpTransport;

            #if UNITY_EDITOR
                if (!ParrelSync.ClonesManager.IsClone())
                {
                    _networkManager.StartServer();
                }
            #else
            _networkManager.StartServer();
            #endif

                _networkManager.StartClient();
            }


        private void StartFromLobby()
        {
            _networkManager.transport = _steamTransport;
            if (!_lobbyDataHolder)
            {
                PurrLogger.LogError("Failed");
                return;
            }
            if (!_lobbyDataHolder.CurrentLobby.IsValid)
            {
                PurrLogger.LogError("Failed");
                return;
            }

            if (!ulong.TryParse(_lobbyDataHolder.CurrentLobby.LobbyId, out ulong ulongId))
            {
                Debug.LogError($"Failed to parse", this);
                return;
            }

            var lobbyOwner = SteamMatchmaking.GetLobbyOwner(new CSteamID(ulongId));
            if (!lobbyOwner.IsValid())
            {
                Debug.LogError($"Failed to get lobby owner", this);
                return;
            }
            _steamTransport.address = lobbyOwner.ToString();
            
            if(_lobbyDataHolder.CurrentLobby.IsOwner)
                _networkManager.StartServer();
            StartCoroutine(StartClient());
        }
        private IEnumerator StartClient()
        {
            yield return new WaitForSeconds(1f);
            _networkManager.StartClient();
        }
    }
