namespace Tests
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;

	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Tests.Listeners;

	[TestClass]
	public class LoggerShould
	{
		private CustomTraceListener listener;

		[TestInitialize]
		public void Setup()
		{
			this.listener = new CustomTraceListener();
			Trace.Listeners.Add(listener);
		}

		[TestMethod]
		public void ProviderShouldBeCreatedWithConfigurationSectionOverrides()
		{
			if (File.Exists("overrided.log"))
			{
				File.Delete("overrided.log");
			}

			var builder = new ConfigurationBuilder();
			builder.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json");
			var configuration = builder.Build();
			var provider = new Log4NetProvider("log4net.config", configuration.GetSection("Logging"));
			var logger = provider.CreateLogger("test");
			logger.LogCritical("Test file creation");

			Assert.IsNotNull(provider);
			Assert.IsTrue(File.Exists("overrided.log"));
		}

		[TestMethod]
		public void LogCriticalMessages()
		{
			var provider = new Log4NetProvider("log4net.config");
			var logger = provider.CreateLogger("Test");

			const string message = "A message";
			logger.LogCritical(message);

			Assert.AreEqual(1, this.listener.Messages.Count);
			Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(message)));
		}

		[TestMethod]
		public void UsePatternLayoutOnExceptions()
		{
			var provider = new Log4NetProvider("log4net.config");
			var logger = provider.CreateLogger("Test");

			try
			{
				ThrowException();
			}
			catch (Exception ex)
			{
				logger.LogCritical(10, ex, "Catched message");
			}

			Assert.AreEqual(1, this.listener.Messages.Count);
			Assert.IsTrue(this.listener.Messages.Any(x => x.Contains("Catched message")));
		}

		/// <summary>
		/// Throws the exception, and have stacktrace to be tested by the ExceptionLayoutPattern.
		/// </summary>
		/// <exception cref="InvalidOperationException">A message</exception>
		private static void ThrowException() => throw new InvalidOperationException("A message");
	}
}