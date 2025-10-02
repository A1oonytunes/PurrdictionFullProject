namespace com.onlineobject.objectnet {
    public interface ITransportPeer {

        ushort GetId();

        string GetAddress();

        ushort GetPort();

        void SetAddress(string address);

        void SetPort(ushort port);

        T GetConnection<T>();

        void SetConnection<T>(T value);

        bool IsConnected();

        void SetConnected(bool value);

        void OnClientConnectedOnPeer(ITransportClient client);

    }
}