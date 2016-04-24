
namespace MaurerConsole
{
	public static class Settings	
	{
		public static int Quantity = SettingsReader.GetSetting<int>("Quantity");
		public static int Prime_BitSize = SettingsReader.GetSetting<int>("Prime.BitSize");		
		public static string File_Output = SettingsReader.GetSettingString("File.Output");
		public static int ThreadSleep_Duration = SettingsReader.GetSetting<int>("ThreadSleep.Duration ");
	}
}
