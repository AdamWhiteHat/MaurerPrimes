
namespace MaurerWinform
{
	public static class Settings
	{
		public static int Default_BitSize = SettingsReader.GetSetting<int>("Default.BitSize");

		public static bool Verbose_Mode = SettingsReader.GetSetting<bool>("Verbose.Mode");
		public static bool Logging_Enabled = SettingsReader.GetSetting<bool>("Logging.Enabled");

		public static string LogFile_Primes = SettingsReader.GetSetting<string>("LogFile.Primes");
		public static string LogFile_Methods = SettingsReader.GetSetting<string>("LogFile.Methods");
		public static string LogFile_Timers = SettingsReader.GetSetting<string>("LogFile.Timers");
		public static string LogFile_Exceptions = SettingsReader.GetSetting<string>("LogFile.Exceptions");

		public static int Search_Depth = SettingsReader.GetSetting<int>("Search.Depth");
		public static int ThreadSleep_Duration = SettingsReader.GetSetting<int>("ThreadSleep.Duration");
	}
}
