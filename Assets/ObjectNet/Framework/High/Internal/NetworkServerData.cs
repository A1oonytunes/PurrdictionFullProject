using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents data for a network server, including its address, online status, and latency.
    /// </summary>
    public class NetworkServerData {

        // Holds the server address information.
        private ServerAddressEntry server;

        // Indicates whether the server is currently online.
        public bool Online = false;

        // Provides the average latency to the server.
        public float Latency { get { return this.GetPingAverage(); } }

        // Reference to the MonoBehaviour that owns this instance, used to start coroutines.
        private MonoBehaviour ownerComponent;

        // Flag to prevent concurrent ping operations.
        private bool isExecutingPing = false;

        // Stores the most recent calculated ping time.
        private float currentPingTime = 0f;

        // A collection of the most recent ping times to calculate the average latency.
        private List<float> latencyTimeSamples = new List<float>();

        // The number of samples to keep for calculating the average ping time.
        const int PING_SAMPLES_AVERAGE = 100;

        // The maximum time to wait for a ping response.
        const float MAX_PING_ANSWER_TIME = 1000f;

        // Default ping time to use when the actual ping time is unknown.
        const float UNKNOW_PING_TIME = 200f;

        /// <summary>
        /// Initializes a new instance of the NetworkServerData class.
        /// </summary>
        /// <param name="owner">The MonoBehaviour that owns this instance.</param>
        /// <param name="info">The server address information.</param>
        public NetworkServerData(MonoBehaviour owner, ServerAddressEntry info) {
            this.ownerComponent = owner;
            this.server = info;
        }

        /// <summary>
        /// Retrieves the server address information.
        /// </summary>
        /// <returns>The server address entry.</returns>
        public ServerAddressEntry GetServerInfo() {
            return this.server;
        }

        /// <summary>
        /// Updates the server address information.
        /// </summary>
        /// <param name="serverData">The new server address data.</param>
        public void SetServerInfo(ServerAddressEntry serverData) {
            this.server = serverData;
        }

        /// <summary>
        /// Registers a new ping time sample and updates the average ping time.
        /// </summary>
        /// <param name="time">The ping time to register.</param>
        public void RegisterPingServerTime(float time) {
            this.latencyTimeSamples.Add(time);
            // To not store too many samples, remove the oldest if we exceed the limit.
            if (this.latencyTimeSamples.Count > PING_SAMPLES_AVERAGE) {
                this.latencyTimeSamples.RemoveAt(0);
            }
            this.currentPingTime = this.CalculatePingTime();
        }

        /// <summary>
        /// Initiates a ping to the server to determine latency and online status.
        /// </summary>
        public void Ping() {
            if (!this.isExecutingPing) {
                if (string.IsNullOrEmpty(this.server.Address) == false) {
                    IEnumerator pingExecution = NetworkDiagnostics.PingAddressAsync(this.server.Address, MAX_PING_ANSWER_TIME, (result, responseTime) => {
                        this.isExecutingPing = false;
                        // Update the online status based on the ping result.
                        this.Online = result;
                        float returnTime = (this.Online == true) ? responseTime : int.MaxValue;
                        // Register the ping time if the server is online.
                        if (this.Online == true) {
                            this.RegisterPingServerTime(returnTime);
                        }
                    });
                    this.ownerComponent.StartCoroutine(pingExecution);
                    this.isExecutingPing = true;
                }
            }
        }

        /// <summary>
        /// Retrieves the average ping time to the server.
        /// </summary>
        /// <returns>The average ping time.</returns>
        private float GetPingAverage() {
            return (this.latencyTimeSamples.Count > 0) ? this.currentPingTime : UNKNOW_PING_TIME;
        }

        /// <summary>
        /// Calculates the average ping time based on collected samples.
        /// </summary>
        /// <returns>The calculated average ping time.</returns>
        private float CalculatePingTime() {
            float pingTime = 0f;
            foreach (float pingSample in this.latencyTimeSamples) {
                pingTime += pingSample;
            }
            return (this.latencyTimeSamples.Count > 0) ? (pingTime / this.latencyTimeSamples.Count) : UNKNOW_PING_TIME;
        }

        /// <summary>
        /// Return if this server is execution a ping 
        /// </summary>
        /// <returns>True f is execution, otherwise false</returns>
        public bool IsExecutionPing() {
            return this.isExecutingPing;
        }

    }


}