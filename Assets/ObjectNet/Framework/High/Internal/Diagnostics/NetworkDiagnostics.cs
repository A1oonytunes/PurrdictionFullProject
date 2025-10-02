using System;
using System.Collections;
#if !UNITY_WEBGL
using UnityEngine;
#endif

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Provides network diagnostics functionality such as host ping and asynchronous address pinging.
    /// </summary>
    public static class NetworkDiagnostics {

        /// <summary>
        /// Gets the host ping time in milliseconds. If a custom ping callback is configured, it will be used; otherwise, a default ping time of 200 milliseconds is returned.
        /// </summary>
        public static float HostPing { get { return (NetworkDiagnostics.pingCallBack != null) ? NetworkDiagnostics.pingCallBack.Invoke() : DEFAULT_PING_TIME; } }

        private static Func<float> pingCallBack;

        private const float DEFAULT_PING_TIME = 200f;

        /// <summary>
        /// Configures a custom ping callback function to be used for obtaining the host ping time.
        /// </summary>
        /// <param name="callback">The callback function that returns the host ping time in milliseconds.</param>
        public static void ConfigurePingCallback(Func<float> callback) {
            NetworkDiagnostics.pingCallBack = callback;
        }

        /// <summary>
        /// Asynchronously pings the specified address and invokes the onFinish action with the result and response time.
        /// </summary>
        /// <param name="addr">The address to ping.</param>
        /// <param name="timeout">The maximum time to wait for a response in seconds.</param>
        /// <param name="onFinish">The action to be invoked with the ping result (true for success, false for failure) and the response time in milliseconds.</param>
        /// <returns>An IEnumerator for asynchronous execution.</returns>
        public static IEnumerator PingAddressAsync(String addr, float timeout, Action<bool, int> onFinish) {
#if UNITY_WEBGL
            yield return null;
            onFinish.Invoke(true, 0); // WebGL doesn't implements ping feature
#else
            bool result = false;
            int responseTime = 0;
            float ellapsedPingTime = 0f;
            // Perform ping
            if ( string.IsNullOrEmpty(addr) == false ) { 
                Ping serverPing = new Ping(addr);
                while ((serverPing.isDone == false) && (ellapsedPingTime < timeout)) {
                    yield return null;
                    ellapsedPingTime += Time.deltaTime;
                }
                // Update server info
                result = serverPing.isDone;
                responseTime = (result == true) ? serverPing.time : int.MaxValue;
                serverPing.DestroyPing();
            } else {
                responseTime = int.MaxValue;
            }

            onFinish.Invoke(result, responseTime);
#endif
        }
    }

}