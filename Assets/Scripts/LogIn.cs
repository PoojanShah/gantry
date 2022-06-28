using System;
using Core;
using Screens;

public static class LogIn
{
	private static bool _isAdminLoggedIn = false;
	private static bool _isSuperAdminLoggedIn = false;
		
	public static void LogInByType(PasswordType type)
	{
		switch (type)
		{
			case PasswordType.Admin:
				_isAdminLoggedIn = true;
				break;
			case PasswordType.SuperAdmin:
				_isSuperAdminLoggedIn = true;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(type), type, null);
		}
	}

	public static bool CheckIsLogInByType(PasswordType type)
	{
		return type switch
		{
			PasswordType.Admin => _isAdminLoggedIn,
			PasswordType.SuperAdmin => _isSuperAdminLoggedIn,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
	}

	public static string GetPasswordByType(PasswordType type)
	{
		return type switch
		{
			PasswordType.Admin => Constants.CorrectAdminPass,
			PasswordType.SuperAdmin => Constants.CorrectSuperPass,
			_ => null
		};
	}
}