// SendArgs.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"


int _tmain(int argc, _TCHAR* argv[])
{

	TCHAR args[256] = {0};

	if (argc == 1)
	{
		wcscat(args, L"/QuickPost");
	}
	else
	{
		wcscat(args, argv[1]);
	}

	HWND hWnd = FindWindow(NULL, L"PockeTwitWndProc");

	if (hWnd)
	{
		COPYDATASTRUCT data;
		ZeroMemory(&data, sizeof(COPYDATASTRUCT));
		data.dwData = 7;
		data.lpData = (PVOID)args;
		data.cbData = (wcslen(args) + 1) * sizeof(TCHAR);

		SendMessage(hWnd, WM_COPYDATA, NULL, (LPARAM)&data);
	}
	else
	{
		TCHAR strPathName[MAX_PATH];
		GetModuleFileName(NULL, strPathName, MAX_PATH);

		wchar_t * pstr = wcsrchr(strPathName, '\\');
		if (pstr) *(++pstr) = '\0';

		wcscat(strPathName, L"PockeTwit.exe");

		SHELLEXECUTEINFO ShExecInfo;
		memset( &ShExecInfo, 0, sizeof( SHELLEXECUTEINFO ) );
		ShExecInfo.cbSize = sizeof(SHELLEXECUTEINFO);
		ShExecInfo.fMask = SEE_MASK_FLAG_NO_UI;
		ShExecInfo.hwnd = NULL;
		ShExecInfo.lpVerb = L"Open";
		ShExecInfo.lpFile = strPathName;
		ShExecInfo.lpParameters = args;
		ShExecInfo.lpDirectory = NULL;
		ShExecInfo.nShow = SW_SHOWNORMAL;
		ShExecInfo.hInstApp = NULL; 
		ShellExecuteEx(&ShExecInfo);
	}

	return 0;
}

