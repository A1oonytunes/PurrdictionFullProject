using System;
using System.Diagnostics;
using System.IO;
using System.Text;
#if !DEBUG
using System.Threading;
#endif
using System.Threading.Tasks;
using System.Xml;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Provides extension methods for various types such as Stream, XmlNode, string, and TraceSource.
    /// </summary>
    internal static class StreamExtensions {
        /// <summary>
        /// Reads a specified number of characters from the current stream and returns the data as a string.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="bytesToRead">The number of characters to read from the stream.</param>
        /// <returns>A string containing the characters read from the stream.</returns>
        internal static string ReadAsMany(this StreamReader stream, int bytesToRead) {
	        var buffer = new char[bytesToRead];
            stream.ReadBlock(buffer, 0, bytesToRead);
            return new string(buffer);
        }

        /// <summary>
        /// Retrieves the inner text of a specified XML element from an XmlNode.
        /// </summary>
        /// <param name="node">The XmlNode to search within.</param>
        /// <param name="elementName">The name of the XML element to find.</param>
        /// <returns>The inner text of the specified XML element, or an empty string if the element is not found.</returns>
        internal static string GetXmlElementText(this XmlNode node, string elementName) {
            XmlElement element = node[elementName];
            return element != null ? element.InnerText : string.Empty;
        }

        /// <summary>
        /// Determines whether a string contains a specified substring, ignoring case.
        /// </summary>
        /// <param name="s">The string to search within.</param>
        /// <param name="pattern">The substring to locate in the string.</param>
        /// <returns>true if the substring occurs within the string; otherwise, false.</returns>
        internal static bool ContainsIgnoreCase(this string s, string pattern) {
            return s.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Logs an informational message to the TraceSource.
        /// </summary>
        /// <param name="source">The TraceSource to write the message to.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal static void LogInfo(this TraceSource source, string format, params object[] args) {
            try {
                source.TraceEvent(TraceEventType.Information, 0, format, args);
            } catch (ObjectDisposedException) {
                // Disable tracing if the TraceSource is disposed.
                source.Switch.Level = SourceLevels.Off;
            }
        }

        /// <summary>
        /// Logs a warning message to the TraceSource.
        /// </summary>
        /// <param name="source">The TraceSource to write the message to.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal static void LogWarn(this TraceSource source, string format, params object[] args) {
            try {
                source.TraceEvent(TraceEventType.Warning, 0, format, args);
            } catch (ObjectDisposedException) {
                // Disable tracing if the TraceSource is disposed.
                source.Switch.Level = SourceLevels.Off;
            }
        }

        /// <summary>
        /// Logs an error message to the TraceSource.
        /// </summary>
        /// <param name="source">The TraceSource to write the message to.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal static void LogError(this TraceSource source, string format, params object[] args) {
            try {
                source.TraceEvent(TraceEventType.Error, 0, format, args);
            } catch (ObjectDisposedException) {
                // Disable tracing if the TraceSource is disposed.
                source.Switch.Level = SourceLevels.Off;
            }
        }


        /// <summary>
        /// Converts the provided XmlDocument into a formatted, human-readable XML string.
        /// </summary>
        /// <param name="document">The XmlDocument to be converted into a string.</param>
        /// <returns>A formatted XML string representation of the XmlDocument.</returns>
        internal static string ToPrintableXml(this XmlDocument document) {
            // Create a memory stream to temporarily hold the XML data.
            using (var stream = new MemoryStream()) {
                // Create an XmlTextWriter with Unicode encoding to write to the memory stream.
                using (var writer = new XmlTextWriter(stream, Encoding.Unicode)) {
                    try {
                        // Set the formatting of the XmlTextWriter to be indented for readability.
                        writer.Formatting = Formatting.Indented;

                        // Write the content of the XmlDocument to the XmlTextWriter.
                        document.WriteContentTo(writer);
                        // Flush the writer's buffer to the underlying stream.
                        writer.Flush();
                        // Flush the stream to make sure all data is written.
                        stream.Flush();

                        // Rewind the MemoryStream to the beginning to prepare for reading.
                        stream.Position = 0;

                        // Create a StreamReader to read the MemoryStream's contents.
                        var reader = new StreamReader(stream);

                        // Read the entire content of the StreamReader and return it as a string.
                        return reader.ReadToEnd();
                    } catch (Exception) {
                        // In case of an exception, return the XmlDocument's string representation.
                        return document.ToString();
                    }
                }
            }
        }

#if NET35
		public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
		{
#if DEBUG
			return task;
#endif
			var timeoutCancellationTokenSource = new CancellationTokenSource();

			return TaskExtension.WhenAny(task, TaskExtension.Delay(timeout, timeoutCancellationTokenSource.Token))
				.ContinueWith(t =>
				{
					Task completedTask = t.Result;

					if (completedTask == task)
					{
						timeoutCancellationTokenSource.Cancel();
						return task;
					}
					throw new TimeoutException(
						"The operation has timed out. The network is broken, router has gone or is too busy.");
				}).Unwrap();
		}
#else
        /// <summary>
        /// Applies a timeout to an asynchronous operation, throwing a TimeoutException if the operation does not complete within the specified TimeSpan.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        /// <param name="task">The task to apply the timeout to.</param>
        /// <param name="timeout">The maximum time to wait for the task to complete.</param>
        /// <returns>The result of the task if it completes in time.</returns>
        /// <exception cref="TimeoutException">Thrown when the task does not complete within the allotted time.</exception>

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
		{
    #if DEBUG
			return await task;
    #else
			// Create a new CancellationTokenSource to handle the timeout
            var timeoutCancellationTokenSource = new CancellationTokenSource();

            // Wait for either the task to complete or the delay to elapse, whichever comes first
            Task completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
            if (completedTask == task) // If the task completed before the timeout
            {
                timeoutCancellationTokenSource.Cancel(); // Cancel the delay task
                return await task; // Return the result of the completed task
            }
            // If the task did not complete before the timeout, throw a TimeoutException
            throw new TimeoutException(
                "The operation has timed out. The network is broken, router has gone or is too busy.");
    #endif
        }
#endif //NET35
    }

#if NET35
	internal static class EnumExtension
	{
		public static bool HasFlag(this Enum value, Enum flag)
		{
			int intValue = (int)(ValueType)value;
			int intFlag = (int)(ValueType)flag;

			return (intValue & intFlag) == intFlag;
		}
	}

	public static class CancellationTokenSourceExtension
	{
		public static void CancelAfter(this CancellationTokenSource source, int millisecondsDelay)
		{
			if (millisecondsDelay < -1)
			{
				throw new ArgumentOutOfRangeException("millisecondsDelay");
			}
			Timer timer = new Timer(self => {
				((Timer)self).Dispose();
				try
				{
					source.Cancel();
				}
				catch (ObjectDisposedException) { }
			});
			timer.Change(millisecondsDelay, -1);
		}
	}

	public static class TaskExtension
	{
		public static Task Delay(TimeSpan delay, CancellationToken token)
		{
			long delayMs = (long)delay.TotalMilliseconds;
			if (delayMs < -1L || delayMs > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("delay");
			}
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

			Timer timer = new Timer(self =>
			{
				tcs.TrySetResult(null); //timer expired, attempt to move task to the completed state.
			}, null, delayMs, -1);

			token.Register(() =>
			{
				timer.Dispose(); //stop the timer
				tcs.TrySetCanceled(); //attempt to mode task to canceled state
			});

			return tcs.Task;
		}

		public static Task<Task> WhenAny(params Task[] tasks)
		{
			return Task.Factory.ContinueWhenAny(tasks, t => t);
		}

		public static Task WhenAll(params Task[] tasks)
		{
			return Task.Factory.ContinueWhenAll(tasks, t => t);
		}
	}
#endif //NET35
}