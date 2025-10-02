using System;
using System.Reflection;
using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// This class represents a steam events
    /// </summary>
    [Serializable]
    public class SteamEventsEntry : ScriptableObject {
        
        [EventInformations(EventName = "OnHostLeaveLobby", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(ulong) }, EventDescriptiom = "Trigger when host player leave lobby")]
        [SerializeField]
        public SteamEventReference onHostLeaveLobby;

        [EventInformations(EventName = "OnBecameHost", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(ulong) }, EventDescriptiom = "Trigger when client became host after migration")]
        [SerializeField]
        public SteamEventReference onBecameHost;

        [EventInformations(EventName = "OnDetectNewHost", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(ulong) }, EventDescriptiom = "Trigger when client detect a new host after migration")]
        [SerializeField]
        public SteamEventReference onDetectNewHost;

        public SteamEventReference GetOnHostLeaveLobby() {
            if (this.onHostLeaveLobby == null) this.onHostLeaveLobby = new SteamEventReference();
            return this.onHostLeaveLobby;
        }

        public SteamEventReference GetOnBecameHost() {
            if (this.onBecameHost == null) this.onBecameHost = new SteamEventReference();
            return this.onBecameHost;
        }

        public SteamEventReference GetOnDetectNewHost() {
            if (this.onDetectNewHost == null) this.onDetectNewHost = new SteamEventReference();
            return this.onDetectNewHost;
        }

        /// <summary>
        /// Execute callback when host leave lobby
        /// </summary>
        public void ExecuteOnHostLeaveLobby(ulong steamid) {
            try { 
                if (this.onHostLeaveLobby != null) {
                    if ((this.onHostLeaveLobby.GetEventTarget() != null) &&
                        (this.onHostLeaveLobby.GetEventComponent() != null) &&
                        (this.onHostLeaveLobby.GetEventMethod() != null)) {
                        MethodInfo executionMethod = this.onHostLeaveLobby.GetEventComponent().GetType().GetMethod(this.onHostLeaveLobby.GetEventMethod());
                        if (executionMethod != null) {
                            executionMethod.Invoke(this.onHostLeaveLobby.GetEventComponent(), new object[] { steamid });
                        }
                    }
                }
            } catch (Exception err) {
                Debug.LogError(err);
            }
        }

        /// <summary>
        /// Execute callback when became host
        /// </summary>
        public void ExecuteOnBecameHost(ulong steamid) {
            try { 
                if (this.onBecameHost != null) {
                    if ((this.onBecameHost.GetEventTarget() != null) &&
                        (this.onBecameHost.GetEventComponent() != null) &&
                        (this.onBecameHost.GetEventMethod() != null)) {
                        MethodInfo executionMethod = this.onBecameHost.GetEventComponent().GetType().GetMethod(this.onBecameHost.GetEventMethod());
                        if (executionMethod != null) {
                            executionMethod.Invoke(this.onBecameHost.GetEventComponent(), new object[] { steamid });
                        }
                    }
                }
            } catch (Exception err) {
                Debug.LogError(err);
            }
        }

        /// <summary>
        /// Execute callback when became host
        /// </summary>
        public void ExecuteOnDetectNewHost(ulong steamid) {
            try {
                if (this.onDetectNewHost != null) {
                    if ((this.onDetectNewHost.GetEventTarget() != null) &&
                        (this.onDetectNewHost.GetEventComponent() != null) &&
                        (this.onDetectNewHost.GetEventMethod() != null)) {
                        MethodInfo executionMethod = this.onDetectNewHost.GetEventComponent().GetType().GetMethod(this.onDetectNewHost.GetEventMethod());
                        if (executionMethod != null) {
                            executionMethod.Invoke(this.onDetectNewHost.GetEventComponent(), new object[] { steamid });
                        }
                    }
                }
            } catch(Exception err) {
                Debug.LogError(err);
            }
        }


    }
}