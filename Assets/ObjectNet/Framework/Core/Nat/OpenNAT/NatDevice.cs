using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a NAT device and provides access to the operation set that allows
    /// open (forward) ports, close ports and get the externa (visible) IP address.
    /// </summary>
    public abstract class NatDevice
	{
		/// <summary>
		/// A local endpoint of NAT device.
		/// </summary>
		public abstract IPEndPoint HostEndPoint { get; }

		/// <summary>
		/// A local IP address of client.
		/// </summary>
		public abstract IPAddress LocalAddress { get; }

		private readonly HashSet<Mapping> _openedMapping = new HashSet<Mapping>();
		protected DateTime LastSeen { get; private set; }

        /// <summary>
        /// Internal touch
        /// </summary>
		internal void Touch()
		{
			LastSeen = DateTime.Now;
		}

		/// <summary>
		/// Creates the port map asynchronous.
		/// </summary>
		/// <param name="mapping">The <see cref="Mapping">Mapping</see> entry.</param>
		/// <example>
		/// device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 1700, 1600));
		/// </example>
		/// <exception cref="MappingException">MappingException</exception>
		public abstract Task CreatePortMapAsync(Mapping mapping);

		/// <summary>
		/// Deletes a mapped port asynchronous.
		/// </summary>
		/// <param name="mapping">The <see cref="Mapping">Mapping</see> entry.</param>
		/// <example>
		/// device.DeletePortMapAsync(new Mapping(Protocol.Tcp, 1700, 1600));
		/// </example>
		/// <exception cref="MappingException">MappingException-class</exception>
		public abstract Task DeletePortMapAsync(Mapping mapping);

		/// <summary>
		/// Gets all mappings asynchronous.
		/// </summary>
		/// <returns>
		/// The list of all forwarded ports
		/// </returns>
		/// <example>
		/// var mappings = await device.GetAllMappingsAsync();
		/// foreach(var mapping in mappings)
		/// {
		///	 Console.WriteLine(mapping)
		/// }
		/// </example>
		/// <exception cref="MappingException">MappingException</exception>
		public abstract Task<IEnumerable<Mapping>> GetAllMappingsAsync();

		/// <summary>
		/// Gets the external (visible) IP address asynchronous. This is the NAT device IP address
		/// </summary>
		/// <returns>
		/// The public IP addrees
		/// </returns>
		/// <example>
		/// Console.WriteLine("My public IP is: {0}", await device.GetExternalIPAsync());
		/// </example>
		/// <exception cref="MappingException">MappingException</exception>
		public abstract Task<IPAddress> GetExternalIPAsync();

		/// <summary>
		/// Gets the specified mapping asynchronous.
		/// </summary>
		/// <param name="protocol">The protocol.</param>
		/// <param name="port">The port.</param>
		/// <returns>
		/// The matching mapping
		/// </returns>
		public abstract Task<Mapping> GetSpecificMappingAsync(Protocol protocol, int port);

        /// <summary>
        /// Registers a new mapping or updates an existing one.
        /// </summary>
        /// <param name="mapping">The mapping to register.</param>
        protected void RegisterMapping(Mapping mapping) {
            // Remove the mapping if it already exists to avoid duplicates
            _openedMapping.Remove(mapping);
            // Add the new or updated mapping
            _openedMapping.Add(mapping);
        }

        /// <summary>
        /// Unregisters a mapping.
        /// </summary>
        /// <param name="mapping">The mapping to unregister.</param>
        protected void UnregisterMapping(Mapping mapping) {
            // Remove the mapping that matches the specified one
            _openedMapping.RemoveWhere(x => x.Equals(mapping));
        }

        /// <summary>
        /// Releases a collection of mappings, attempting to close their associated ports.
        /// </summary>
        /// <param name="mappings">The collection of mappings to release.</param>
        internal void ReleaseMapping(IEnumerable<Mapping> mappings) {
            // Convert the collection to an array for processing
            var maparr = mappings.ToArray();
            var mapCount = maparr.Length;
            // Log the number of ports to close
            NatDiscoverer.TraceSource.LogInfo("{0} ports to close", mapCount);
            for (var i = 0; i < mapCount; i++) {
                var mapping = _openedMapping.ElementAt(i);

                try {
                    // Attempt to delete the port mapping asynchronously
                    DeletePortMapAsync(mapping);
                    // Log success
                    NatDiscoverer.TraceSource.LogInfo(mapping + " port successfully closed");
                } catch (Exception) {
                    // Log failure
                    NatDiscoverer.TraceSource.LogError(mapping + " port couldn't be close");
                }
            }
        }

        /// <summary>
        /// Releases all currently opened mappings.
        /// </summary>
        internal void ReleaseAll() {
            // Release all mappings in the _openedMapping collection
            ReleaseMapping(_openedMapping);
        }

        /// <summary>
        /// Releases all mappings that have a lifetime type of 'Session'.
        /// </summary>
        internal void ReleaseSessionMappings() {
            // Select all mappings with a 'Session' lifetime type
            var mappings = from m in _openedMapping
                           where m.LifetimeType == MappingLifetime.Session
                           select m;

            // Release the selected mappings
            ReleaseMapping(mappings);
        }

#if NET35
        /// <summary>
        /// Renews all mappings that should be renewed, for .NET 3.5.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        internal Task RenewMappings()
        {
            Task task = null;
            // Select all mappings that should be renewed
            var mappings = _openedMapping.Where(x => x.ShoundRenew());
            foreach (var mapping in mappings.ToArray())
            {
                var m = mapping;
                // Chain the renewal tasks
                task = task == null ? RenewMapping(m) : task.ContinueWith(t => RenewMapping(m)).Unwrap();
            }

            return task;
        }
#else
        /// <summary>
        /// Renews all mappings that should be renewed, for .NET versions above 3.5.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        internal async Task RenewMappings() {
            // Select all mappings that should be renewed
            var mappings = _openedMapping.Where(x => x.ShouldRenew());
            foreach (var mapping in mappings.ToArray()) {
                var m = mapping;
                // Renew each mapping asynchronously
                await RenewMapping(m);
            }
        }
#endif

#if NET35
        /// <summary>
        /// Renews a single mapping for .NET 3.5.
        /// </summary>
        /// <param name="mapping">The mapping to renew.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private Task RenewMapping(Mapping mapping)
        {
            var renewMapping = new Mapping(mapping);
            renewMapping.Expiration = DateTime.UtcNow.AddSeconds(mapping.Lifetime);

            // Log the renewal attempt
            NatDiscoverer.TraceSource.LogInfo("Renewing mapping {0}", renewMapping);
            return CreatePortMapAsync(renewMapping)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        // Log a warning if the renewal fails
                        NatDiscoverer.TraceSource.LogWarn("Renew {0} failed", mapping);
                    }
                    else
                    {
                        // Log the next scheduled renewal time
                        NatDiscoverer.TraceSource.LogInfo("Next renew scheduled at: {0}",
                                                            renewMapping.Expiration.ToLocalTime().TimeOfDay);
                    }
                });
        }
#else
        /// <summary>
        /// Renews a single mapping for .NET versions above 3.5.
        /// </summary>
        /// <param name="mapping">The mapping to renew.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task RenewMapping(Mapping mapping) {
            var renewMapping = new Mapping(mapping);
            try {
                renewMapping.Expiration = DateTime.UtcNow.AddSeconds(mapping.Lifetime);

                // Log the renewal attempt
                NatDiscoverer.TraceSource.LogInfo("Renewing mapping {0}", renewMapping);
                await CreatePortMapAsync(renewMapping);
                // Log the next scheduled renewal time
                NatDiscoverer.TraceSource.LogInfo("Next renew scheduled at: {0}",
                                                    renewMapping.Expiration.ToLocalTime().TimeOfDay);
            } catch (Exception) {
                // Log a warning if the renewal fails
                NatDiscoverer.TraceSource.LogWarn("Renew {0} failed", mapping);
            }
        }
#endif

    }
}