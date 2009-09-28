#include "stdafx.h"
#include <windows.h>
#include <ce_setup.h>

codeINSTALL_INIT Install_Init(
    HWND        hwndParent,
    BOOL        fFirstCall, 
    BOOL        fPreviouslyInstalled,
    LPCTSTR     pszInstallDir
    )
{
    return codeINSTALL_INIT_CONTINUE;
}

codeINSTALL_EXIT Install_Exit(
    HWND    hwndParent,
    LPCTSTR pszInstallDir,
    WORD    cFailedDirs,
    WORD    cFailedFiles,
    WORD    cFailedRegKeys,
    WORD    cFailedRegVals,
    WORD    cFailedShortcuts
    )
{
	PROCESS_INFORMATION pi      = {0};
    codeINSTALL_EXIT    cie     = codeINSTALL_EXIT_DONE;

	// We are provided with the installation folder the
	// user has installed the application into. So prepend
	// the name of the application we want to launch.
	TCHAR szPath[MAX_PATH];
	_tcscpy(szPath, pszInstallDir);
	_tcscat(szPath, _T("\\"));
	_tcscat(szPath, _T("PockeTwit.exe"));

	// Refresh TodayScreen to show plugin
	PostMessage(HWND_BROADCAST, WM_WININICHANGE, 0xF2, 0);
	
	// Start the application, and don't wait for it to exit
    if (!CreateProcess(szPath, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, &pi))
	{
		MessageBox(GetForegroundWindow(), szPath, L"Failed to start PockeTwit", MB_OK);
	    cie = codeINSTALL_EXIT_UNINSTALL;
	}

	return cie;
}



codeUNINSTALL_INIT Uninstall_Init(
    HWND        hwndParent,
    LPCTSTR     pszInstallDir
    )
{
	// Disable Today Plugin
	HKEY key;
	DWORD dwEnabled, dwRet;
	DWORD lpcbData = sizeof(dwEnabled);
	
	dwRet = RegOpenKeyEx(HKEY_LOCAL_MACHINE,_T("\\Software\\Microsoft\\Today\\Items\\PockeTwit"),0,0,&key);
	
	dwEnabled = 0;
	
	dwRet = RegSetValueEx(key,_T("Enabled"),0,REG_DWORD,(LPBYTE)&dwEnabled,sizeof(DWORD));
	
	RegFlushKey(key);

	RegCloseKey(key);

	PostMessage(HWND_BROADCAST, WM_WININICHANGE, 0xF2, 0);

	Sleep(1000);

    return codeUNINSTALL_INIT_CONTINUE;
}



codeUNINSTALL_EXIT Uninstall_Exit(
    HWND    hwndParent
    )
{
    return codeUNINSTALL_EXIT_DONE;
}

