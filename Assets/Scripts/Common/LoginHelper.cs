using Core;

namespace Common
{
	public static class LoginHelper
	{
		private static bool _isAdminLoggedIn;

		public static void SaveLogin() => _isAdminLoggedIn = true;
		public static bool IsLoggedIn() => _isAdminLoggedIn;
		public static string GetPassword() => Constants.CorrectAdminPassword;
	}
}