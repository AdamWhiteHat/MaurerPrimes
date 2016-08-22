using System;
using System.Linq;
using System.Configuration;

namespace MaurerWinform
{
	public static class SettingsReader
	{
		public static T GetSetting<T>(string SettingName)
		{
			try
			{
				if (!SettingExists(SettingName))
					return default(T);
				else
					return (T)Convert.ChangeType(ConfigurationManager.AppSettings[SettingName], typeof(T));
			}
			catch
			{
				return default(T);
			}
		}

		private static bool SettingExists(string SettingName, bool CheckForEmptyValue = true)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(SettingName))
					return false;

				if (!ConfigurationManager.AppSettings.AllKeys.Contains(SettingName))
					return false;

				if (CheckForEmptyValue)
				{
					if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[SettingName]))
						return false;
				}

				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
