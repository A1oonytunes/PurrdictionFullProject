#if STEAMWORKS_NET
using Steamworks;
#endif
using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a Steam Lobby object to be used as basic steam structure
    /// </summary>
    [Serializable]
    public struct SteamLobby
#if STEAMWORKS_NET
        : IEquatable<CSteamID>, IEquatable<ulong>, IEquatable<SteamLobby> {
#else
        {
#endif

#if STEAMWORKS_NET
        private ulong id;

        /// <summary>
        /// The native <see cref="CSteamID"/> of the lobby
        /// </summary>
        public readonly CSteamID SteamId {
            get => new(id);
        }

        /// <summary>
        /// Standard metadata field representing the name of the lobby.
        /// </summary>
        public const string DataName = "name";
        /// <summary>
        /// The version of the game.
        /// </summary>
        public const string DataVersion = "objectNetGameVersion";
        /// <summary>
        /// Indicate that the user is ready to play.
        /// </summary>
        public const string DataReady = "objectNetReady";
        /// <summary>
        /// Indicate the mode of the lobby e.g. group or general
        /// </summary>
        public const string DataMode = "objectNetMode";
        /// <summary>
        /// Indicate the type of lobby e.g. private, friend, public or invisible
        /// </summary>
        public const string DataType = "objectNetType";

        public int CompareTo(CSteamID other) {
            return id.CompareTo(other);
        }

        public int CompareTo(ulong other) {
            return id.CompareTo(other);
        }

        public bool Equals(CSteamID other) {
            return id.Equals(other);
        }

        public bool Equals(ulong other) {
            return id.Equals(other);
        }

        public override bool Equals(object obj) {
            return id.Equals(obj);
        }

        public override int GetHashCode() {
            return id.GetHashCode();
        }

        public bool Equals(SteamLobby other) {
            return id.Equals(other.id);
        }

        /// <summary>
        /// Is the user the host of this lobby
        /// </summary>        
        public readonly bool IsOwner {
            get {
                return SteamUser.GetSteamID() == SteamMatchmaking.GetLobbyOwner(this);
            }
        }

        /// <summary>
        /// Indicates rather or not this lobby is a party lobby
        /// </summary>
        public readonly bool IsSession {
            get {
                return this[DataMode] == "Session";
            }
            set {
                if (IsOwner) {
                    if (value) {
                        this[DataMode] = "Session";
                    } else {
                        this[DataMode] = "General";
                    }
                }
            }
        }

        /// <summary>
        /// Gets a lobby based on a provided account aka friend ID
        /// </summary>
        /// <param name="accountId">The ID of the lobby as a string to return</param>
        /// <returns>The related lobby</returns>
        public static SteamLobby Get(string accountId) {
            uint id = Convert.ToUInt32(accountId, 16);
            if (id > 0)
                return Get(id);
            else
                return CSteamID.Nil;
        }
        /// <summary>
        /// Get the lobby represented by this account ID
        /// </summary>
        /// <param name="accountId">The account ID of the lobby to return</param>
        /// <returns>The related lobby</returns>
        public static SteamLobby Get(uint accountId) => new CSteamID(new AccountID_t(accountId), 393216, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeChat);
        /// <summary>
        /// Get the lobby represented by this account ID
        /// </summary>
        /// <param name="accountId">The account ID of the lobby to return</param>
        /// <returns>The related lobby</returns>
        public static SteamLobby Get(AccountID_t accountId) => new CSteamID(accountId, 393216, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeChat);
        /// <summary>
        /// Get the lobby represented by this CSteamID value
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SteamLobby Get(ulong id) => new SteamLobby { id = id };
        /// <summary>
        /// Get the lobby represented by this CSteamID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SteamLobby Get(CSteamID id) => new SteamLobby { id = id.m_SteamID };

        /// <summary>
        /// Read and write metadata values to the lobby
        /// </summary>
        /// <param name="metadataKey">The key of the value to be read or writen</param>
        /// <returns>The value of the key if any otherwise returns and empty string.</returns>
        public readonly string this[string metadataKey] {
            get {
                return SteamMatchmaking.GetLobbyData(this.SteamId, metadataKey);
            }
            set {
                SteamMatchmaking.SetLobbyData(this.SteamId, metadataKey, value);
            }
        }

        public static bool operator ==(SteamLobby l, SteamLobby r) => l.id == r.id;
        public static bool operator ==(CSteamID l, SteamLobby r) => l.m_SteamID == r.id;
        public static bool operator ==(SteamLobby l, CSteamID r) => l.id == r.m_SteamID;
        public static bool operator ==(SteamLobby l, ulong r) => l.id == r;
        public static bool operator ==(ulong l, SteamLobby r) => l == r.id;
        public static bool operator !=(SteamLobby l, SteamLobby r) => l.id != r.id;
        public static bool operator !=(CSteamID l, SteamLobby r) => l.m_SteamID != r.id;
        public static bool operator !=(SteamLobby l, CSteamID r) => l.id != r.m_SteamID;
        public static bool operator !=(SteamLobby l, ulong r) => l.id != r;
        public static bool operator !=(ulong l, SteamLobby r) => l != r.id;

        public static implicit operator CSteamID(SteamLobby c) => c.SteamId;
        public static implicit operator SteamLobby(CSteamID id) => new SteamLobby { id = id.m_SteamID };
        public static implicit operator ulong(SteamLobby id) => id.id;
        public static implicit operator SteamLobby(ulong id) => new SteamLobby { id = id };
        public static implicit operator SteamLobby(AccountID_t id) => Get(id);
        public static implicit operator SteamLobby(uint id) => Get(id);
        public static implicit operator SteamLobby(string id) => Get(id);

#endif
    }
}