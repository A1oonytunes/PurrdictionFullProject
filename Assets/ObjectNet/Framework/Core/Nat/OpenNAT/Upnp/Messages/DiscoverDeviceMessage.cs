using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace com.onlineobject.objectnet {
	internal static class DiscoverDeviceMessage
	{
		/// <summary>
		/// The message sent to discover all uPnP devices on the network
		/// </summary>
		/// <returns></returns>
		public static string Encode(string serviceType, IPAddress address)
		{
			var fmtAddress = string.Format(
				address.AddressFamily == AddressFamily.InterNetwork ? "{0}" : "[{0}]",
				address);

			string s = "M-SEARCH * HTTP/1.1\r\n"
						+ "HOST: " + fmtAddress + ":1900\r\n"
						+ "MAN: \"ssdp:discover\"\r\n"
						+ "MX: 3\r\n"
						+ "ST: urn:schemas-upnp-org:service:{0}\r\n\r\n";

			return string.Format(CultureInfo.InvariantCulture, s, serviceType);
		}
	}
}