using System;

namespace com.onlineobject.objectnet.voice {
    public struct ObjectNetVoicePeer : IEquatable<ObjectNetVoicePeer> {
        
        public readonly int clientId;

        public ObjectNetVoicePeer(int id) {
            clientId = id;
        }

        public bool Equals(ObjectNetVoicePeer other) {
            return clientId == other.clientId;
        }

        public override bool Equals(object obj) {
            return obj is ObjectNetVoicePeer other && Equals(other);
        }

        public override int GetHashCode() {
            return clientId.GetHashCode();
        }

        public static bool operator ==(ObjectNetVoicePeer left, ObjectNetVoicePeer right) {
            return left.Equals(right);
        }

        public static bool operator !=(ObjectNetVoicePeer left, ObjectNetVoicePeer right) {
            return !left.Equals(right);
        }
    }
}