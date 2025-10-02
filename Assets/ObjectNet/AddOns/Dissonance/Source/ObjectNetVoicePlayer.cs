#if DISSONANCE_ENABLED
using Dissonance;
using UnityEngine;
#else
using UnityEngine;
#endif


namespace com.onlineobject.objectnet.voice {
	public class ObjectNetVoicePlayer : NetworkBehaviour
#if DISSONANCE_ENABLED
	, IDissonancePlayer
#endif	
    {
#if DISSONANCE_ENABLED

		private static readonly Log Log = Logs.Create(LogCategory.Network, "ObjectNet Player Component");

		private DissonanceComms _comms;

		public Vector3 Position => transform.position;

		public Quaternion Rotation => transform.rotation;


		private NetworkVariable<string> _playerId = "";

        public string PlayerId { get { return _playerId; }}

		public NetworkPlayerType Type
		{
			get
			{
				if (_comms == null || string.IsNullOrEmpty(_playerId))
					return NetworkPlayerType.Unknown;
				return _comms.LocalPlayerName.Equals(_playerId) ? NetworkPlayerType.Local : NetworkPlayerType.Remote;
			}
		}


		public bool IsTracking { get; private set; }

		private bool wasDisabled = false;

		public void OnEnable() {
			_comms = GameObject.FindAnyObjectByType<DissonanceComms>();

			// Retrack the player if it was diabled before
			if (wasDisabled) {
				if (!string.IsNullOrEmpty(_playerId))
					SetPlayerName(_playerId);
				wasDisabled = false;
			}


		}

		public void OnDisable() {
			if (IsTracking)
				StopTracking();
			wasDisabled = true;
		}

		private void OnDestroy() {
			if (_comms != null)
				_comms.LocalPlayerNameChanged -= SetPlayerName;
			if (IsTracking)
				StopTracking();
		}


		public override void OnNetworkStarted() {
			base.OnNetworkStarted();

			_playerId.OnValueChange(OnNetworkVariablePlayerIdChanged);
		}

		public void ActiveStart() {

			var comms = GameObject.FindAnyObjectByType<DissonanceComms>();


			if (comms == null)
			{
				Log.Error("cannot find DissonanceComms component in scene");
				return;
			}

			Log.Debug("Tracking `OnStartLocalPlayer` Name={0}", comms.LocalPlayerName);

			// This method is called on the client which has control authority over this object. This will be the local client of whichever player we are tracking.
			if (comms.LocalPlayerName != null)
				SetPlayerName(comms.LocalPlayerName);

			//Subscribe to future name changes (this is critical because we may not have run the initial set name yet and this will trigger that initial call)
			comms.LocalPlayerNameChanged += SetPlayerName;
		}

		public void PassiveStart() {
			//A Passive is starting. Start tracking if the name has been properly initialised.
			if (!string.IsNullOrEmpty(PlayerId))
				StartTracking();
		}

		private void SetPlayerName(string playerName) {
			//We need the player name to be set on all the clients and then tracking to be started (on each client).
			//To do this we send a command from this client, informing the server of our name. The server will pass this on to all the clients (with an RPC)

			//We need to stop and restart tracking to handle the name change
			if (IsTracking)
				StopTracking();
			//Perform the actual work
			_playerId = playerName;
			StartTracking();

		}


		private void OnNetworkVariablePlayerIdChanged(string previousvalue, string newvalue) {
			_playerId = newvalue;
			if (IsTracking)
				StopTracking();
			StartTracking();
		}


		private void StartTracking() {
			if (IsTracking)
				throw Log.CreatePossibleBugException("Attempting to start player tracking, but tracking is already started", "B7D1F25E-72AF-4E93-8CFF-90CEBEAC68CF");

			if (_comms != null) {
				_comms.TrackPlayerPosition(this);
				IsTracking = true;
			}
		}

		private void StopTracking() {
			if (!IsTracking)
				throw Log.CreatePossibleBugException("Attempting to stop player tracking, but tracking is not started", "EC5C395D-B544-49DC-B33C-7D7533349134");

			if (_comms != null) {
				_comms.StopTracking(this);
				IsTracking = false;
			}
		}

#endif
	}
}
