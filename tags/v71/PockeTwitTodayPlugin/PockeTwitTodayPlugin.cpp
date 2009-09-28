
#include "stdafx.h"
#include <todaycmn.h>
#include "resource.h"
#include "PockeTwitTodayPlugin.h"
#include "snapi.h"
#include "Regext.h"
#include <shellapi.h>

#ifndef WS_EX_TRANSPARENT
#define WS_EX_TRANSPARENT 0x00000020L
#endif

HINSTANCE g_hinstDLL;

#define PLUGIN_SELECTED     0x00000001
#define PLUGIN_TEXTRESIZE   0x00000002

#define TEXT_LEFT_MARGIN  SCALEY(2)
#define TEXT_RIGHT_MARGIN  SCALEY(2)

HWND hwnd;

TCHAR g_szPluginClass[] = TEXT("PockeTwitTodayPlugin");
DWORD g_cyItemHeight;
DWORD g_cyInitItemHeight;
uint g_uMetricChangeMsg2;
DWORD g_cyDefaultItemHeight;

BOOL VGA = FALSE;

HICON g_hIcon =  NULL;

HREGNOTIFY hrUnreadCountChanged = NULL;

UnreadCount *unreadCountPointer;
DWORD unreadGroupsCount;
DWORD unreadGroupsCurrent = 0;
BOOL hasUnreadGroups;

void InitPluginGlobals(void)
{
    // Plug-in height
    g_uMetricChangeMsg2 = RegisterWindowMessage(SH_UIMETRIC_CHANGE);

	if (DRA::GetScreenCaps(LOGPIXELSY) == 192)
		VGA = TRUE;
	else
		VGA = FALSE;

	if (VGA)
		g_hIcon = LoadIcon(g_hinstDLL, MAKEINTRESOURCE(IDI_ICON32));
	else
		g_hIcon = LoadIcon(g_hinstDLL, MAKEINTRESOURCE(IDI_ICON16));

	g_cyDefaultItemHeight = SCALEY(20);

    g_cyItemHeight = g_cyDefaultItemHeight;
    g_cyInitItemHeight = g_cyItemHeight;
}

void SetItemState(HWND hwnd, LONG lState, BOOL fSet)
{
    LONG lData = GetWindowLong(hwnd, GWL_USERDATA);
    if (fSet)
    {
        lData |= lState;
    }
    else
    {
        lData &= ~lState;
    }
    SetWindowLong(hwnd, GWL_USERDATA, lData);
}

BOOL IsItemState(HWND hwnd, LONG lState)
{
    return ((GetWindowLong(hwnd, GWL_USERDATA) & lState) == lState);
}

void FillRectClr(HDC hdc, LPRECT prc, COLORREF clr)
{
    COLORREF clrSave = SetBkColor(hdc, clr);
    ExtTextOut(hdc,0,0,ETO_OPAQUE,prc,NULL,0,NULL);
    SetBkColor(hdc, clrSave);
}

void Plugin_OnPaint(HWND hwnd, HDC hdc)
{
    HWND    hwndParent = GetParent(hwnd);
    RECT    rcDraw, rcText;
    TODAYDRAWWATERMARKINFO dwi = {0};
    COLORREF crText, crHighlightedText, crOld;
    int nBkMode;
	LOGFONT lf;
    HFONT hSysFont;
    HFONT hFont;
	HFONT hFontOld;
	HDC hdcScreenBuffer;
	HBITMAP memBM;
	HGDIOBJ oldMem;

    ASSERT(NULL != hdc);
	
    GetClientRect(hwnd, &rcDraw);
	GetClientRect(hwnd, &rcText);
    
	hdcScreenBuffer = CreateCompatibleDC(hdc);
	memBM = CreateCompatibleBitmap( hdc, rcDraw.right, rcDraw.bottom );
	oldMem = SelectObject(hdcScreenBuffer, memBM);

    dwi.rc = rcDraw;
    dwi.hwnd = hwnd;
    dwi.hdc = hdcScreenBuffer;
	
    if (IsItemState(hwnd, PLUGIN_SELECTED))
    {
        COLORREF crHighlight;

        crHighlight = SendMessage(hwndParent, TODAYM_GETCOLOR, TODAYCOLOR_HIGHLIGHT, 0);
        
        nBkMode = SetBkMode(hdcScreenBuffer, OPAQUE);
        FillRectClr(hdcScreenBuffer, &rcDraw, crHighlight);
        SetBkMode(hdcScreenBuffer, nBkMode);
    }
    else
    {
        if (!SendMessage(hwndParent, TODAYM_DRAWWATERMARK, 0, (LPARAM) &dwi))
        {
            FillRectClr(hdcScreenBuffer, &rcDraw, GetSysColor(COLOR_WINDOW));
        }
    }
    
	if (VGA)
		rcText.left = rcText.left + 24 + 32;
	else
		rcText.left = rcText.left + 12 + 16;

    rcText.right -= TEXT_RIGHT_MARGIN;
	rcText.bottom = g_cyDefaultItemHeight;
	
    crHighlightedText = SendMessage(hwndParent, TODAYM_GETCOLOR, TODAYCOLOR_HIGHLIGHTEDTEXT, 0);
    crText = SendMessage(hwndParent, TODAYM_GETCOLOR, TODAYCOLOR_TEXT, 0);

    nBkMode = SetBkMode(hdcScreenBuffer, TRANSPARENT);
    crOld = SetTextColor(hdcScreenBuffer, IsItemState(hwnd, PLUGIN_SELECTED) ? crHighlightedText : crText);

	DrawIcon(hdcScreenBuffer, TEXT_LEFT_MARGIN, SCALEY(2), g_hIcon);
   
    hSysFont = (HFONT) GetStockObject(SYSTEM_FONT);
    GetObject(hSysFont, sizeof(LOGFONT), &lf);
    
    lf.lfWeight = FW_NORMAL;
    // Calculate the font size, making sure to round the result to the nearest integer
    lf.lfHeight = (long) -((8.0 * (double)GetDeviceCaps(hdcScreenBuffer, LOGPIXELSY) / 72.0)+.5);
    
    // create the font
    hFont = CreateFontIndirect(&lf);
    
    // Select the system font into the device context
    hFontOld = (HFONT) SelectObject(hdcScreenBuffer, hFont);

	if (hasUnreadGroups)
	{
		WCHAR msgbuf[128] = _T("");
		wcscat(msgbuf, unreadCountPointer[unreadGroupsCurrent].GroupName);
		wcscat(msgbuf, _T(" ("));
		wcscat(msgbuf, unreadCountPointer[unreadGroupsCurrent].swUnread);
		wcscat(msgbuf, _T(")"));
		DrawText(hdcScreenBuffer, msgbuf, -1, &rcText, DT_VCENTER | DT_LEFT | DT_SINGLELINE | DT_NOPREFIX );
	}
	else
	{
		LPCTSTR pszText = (LPCTSTR) LoadString(g_hinstDLL, IDS_PLUGIN_TEXT, NULL, 0);
		DrawText(hdcScreenBuffer, pszText, -1, &rcText, DT_VCENTER | DT_LEFT | DT_SINGLELINE | DT_NOPREFIX );
	}

	// Select the previous font back into the device context
    SelectObject(hdcScreenBuffer, hFontOld);
    
    DeleteObject(hFont);

	crText = SetTextColor(hdcScreenBuffer, crOld);
    SetBkMode(hdcScreenBuffer, nBkMode);

	SelectObject(hdcScreenBuffer, memBM); 
	BitBlt(hdc, 0, 0, rcDraw.right, rcDraw.bottom, hdcScreenBuffer, 0, 0, SRCCOPY) ; 

	SelectObject(hdcScreenBuffer, oldMem);

	DeleteObject(memBM);
	DeleteObject(oldMem);
	DeleteDC(hdcScreenBuffer) ;

    return;
}

BOOL Plugin_OnQueryRefreshCache(HWND hwnd, TODAYLISTITEM* ptli)
{
    // Hmm.... no ptli, that means we are probably refreshing ourselves, and don't
    // need to return antyhing.
    if(!ptli)
    {
        return FALSE;
    }

    if(ptli->cyp != g_cyItemHeight)
    {
        // If the heights are not equal, return true so we tell the Today screen our
        // height has changed.
        OutputDebugString(TEXT("***first time...***"));
        ptli->cyp = g_cyItemHeight;
        InvalidateRect(hwnd, NULL, FALSE);
        return TRUE;
    }
    else if (IsItemState(hwnd, PLUGIN_TEXTRESIZE))
    {
        if (ptli->cyp > g_cyInitItemHeight)
        {
            OutputDebugString(TEXT("***shrinking...***"));
            g_cyItemHeight = g_cyInitItemHeight;
            ptli->cyp = g_cyItemHeight;
        }
        else
        {
            OutputDebugString(TEXT("***growing...***"));
            g_cyItemHeight = g_cyInitItemHeight + g_cyDefaultItemHeight;
            ptli->cyp = g_cyItemHeight;
        }

        SetItemState(hwnd, PLUGIN_TEXTRESIZE, FALSE);
        InvalidateRect(hwnd, NULL, FALSE);
        return TRUE;        
    }
    else
    {
        return FALSE;
    }
}

BOOL Plugin_OnReceivedSelection(HWND hwnd, UINT vKey)
{
    BOOL fRet = FALSE;

    switch(vKey)
    {
        case VK_UP:
        case VK_DOWN:
            SetItemState(hwnd, PLUGIN_SELECTED, TRUE);
            fRet = TRUE;
            break;

        case 0:
            break;
            
        default:
            ASSERT(0);
    }
    return fRet;
}

void Plugin_OnLostSelection(HWND hwnd)
{
    SetItemState(hwnd, PLUGIN_SELECTED, FALSE);
    InvalidateRect(hwnd, NULL, FALSE);
}

LRESULT PluginWndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    LRESULT lRet = 0;

    switch(uMsg)
    {
        case WM_CREATE:
        {
            SetWindowLong(hwnd, GWL_USERDATA, 0);
            InitPluginGlobals();
            break;
        }
        
        case WM_PAINT:
        {
            PAINTSTRUCT ps;
            HDC hDC = BeginPaint(hwnd, &ps);
            Plugin_OnPaint(hwnd, hDC);
            EndPaint(hwnd, &ps);
            break;            
        }

        case WM_LBUTTONDOWN:
            PostMessage(GetParent(hwnd), TODAYM_TOOKSELECTION, (WPARAM)hwnd, 0);
            SetItemState(hwnd, PLUGIN_SELECTED, TRUE);
			InvalidateRect(hwnd, NULL, FALSE);
            break;
		
		case WM_TODAYCUSTOM_ACTION:
		case WM_LBUTTONUP:
			StartPockeTwit();
			break;

		case WM_TIMER:
            switch (wParam)	
			{
				case IDT_TIMER_NEXT_GROUP:

					unreadGroupsCurrent++;
					if (unreadGroupsCurrent >= unreadGroupsCount)
					{
						unreadGroupsCurrent = 0;
					}
					
					SelectNextUnreadGroup();

					InvalidateRect(hwnd, NULL, FALSE);

					break;
			}
            break;

        case WM_ERASEBKGND:
            lRet = 1;
            break;

        case WM_TODAYCUSTOM_QUERYREFRESHCACHE:
            lRet = Plugin_OnQueryRefreshCache(hwnd, (TODAYLISTITEM*) wParam);
            break;

        case WM_TODAYCUSTOM_RECEIVEDSELECTION:
            lRet = Plugin_OnReceivedSelection(hwnd, wParam);
            break;

        case WM_TODAYCUSTOM_LOSTSELECTION:
            Plugin_OnLostSelection(hwnd);
            break;
            
        case WM_TODAYCUSTOM_CLEARCACHE:
        case WM_TODAYCUSTOM_USERNAVIGATION:
        default:
            if (uMsg == g_uMetricChangeMsg2)
            {
                //user changed the Text Size.  we should resize this plugin in the next WM_TODAYCUSTOM_QUERYREFRESHCACHE
                SetItemState(hwnd, PLUGIN_TEXTRESIZE, TRUE);
            }
            lRet = DefWindowProc(hwnd, uMsg, wParam, lParam);
    }

    return lRet;
}


HWND InitializeCustomItem(TODAYLISTITEM * ptli, HWND hwndParent)
{
    WNDCLASS wc;

    if (!GetClassInfo(g_hinstDLL, g_szPluginClass, &wc))
    {
        // Class is not registered yet. Register now...
        memset(&wc, 0, sizeof(wc));

        wc.style         = 0;                  
        wc.lpfnWndProc   = (WNDPROC)PluginWndProc;
        wc.hInstance     = g_hinstDLL;
        wc.hIcon         = NULL;
        wc.hCursor       = 0;
        wc.hbrBackground = NULL;
        wc.lpszClassName = g_szPluginClass;

        // register our window
        if(!RegisterClass(&wc))
        { 
            return NULL;
        }
    }

    //create a new window
    hwnd = CreateWindowEx(WS_EX_TRANSPARENT,
                          g_szPluginClass, 
                          g_szPluginClass,
                          WS_CHILD,   
                          0,0,0,0,
                          hwndParent, NULL, g_hinstDLL, NULL);

	hrUnreadCountChanged = RegisterUnreadCountChangedCallback();

	GetDataFromRegistry();

    //display the window
    if(ptli->fEnabled)
    {
        ShowWindow (hwnd, SW_SHOW);
		SetTimer(hwnd,IDT_TIMER_NEXT_GROUP, 5000 ,NULL);
    }

    return hwnd;

}

EXTERN_C
BOOL __stdcall DllMain(void * hmod, DWORD dwReason, void* lpvReserved)
{
    switch (dwReason)
    {
        case DLL_PROCESS_ATTACH:
            g_hinstDLL = (HINSTANCE)hmod;
            break;
        case DLL_PROCESS_DETACH:
			RegistryCloseNotification(hrUnreadCountChanged);
			KillTimer(hwnd, IDT_TIMER_NEXT_GROUP);
            UnregisterClass(g_szPluginClass, g_hinstDLL);
			DestroyIcon(g_hIcon);
            g_hIcon = NULL;
            g_hinstDLL = NULL;
            break;
        case DLL_THREAD_ATTACH:
			break;
        case DLL_THREAD_DETACH:
			break;
        default:
            break;
   }

    return TRUE;
}


HREGNOTIFY RegisterUnreadCountChangedCallback()
{

    NOTIFICATIONCONDITION nc;
    HREGNOTIFY hNotify = NULL;

    nc.dwMask = 0;
    nc.ctComparisonType = REG_CT_ANYCHANGE;
    nc.TargetValue.psz = _T("");
    
    RegistryNotifyCallback(HKEY_LOCAL_MACHINE,
                                _T("Software\\Apps\\JustForFun PockeTwit\\UnreadCount"), 
                                _T("UnreadCountChanged"), 
                                UnreadCountChangedCallback, 
                                0, 
                                &nc, 
                                &hNotify);
	
    // Close the notification using RegistryCloseNotification when done.
    // Note that it is alright to call RegistryCloseNotification from the callback function.
    // hr = RegistryCloseNotification(hNotify);

    return hNotify;
}

// This is the callback function.
void UnreadCountChangedCallback(HREGNOTIFY hNotify, DWORD dwUserData, const PBYTE pData, const UINT cbData)
{
	KillTimer(hwnd, IDT_TIMER_NEXT_GROUP);
	
	GetDataFromRegistry();
	
	SelectNextUnreadGroup();
	
	SetTimer(hwnd,IDT_TIMER_NEXT_GROUP, 5000 ,NULL);
	
	InvalidateRect(hwnd, NULL, FALSE);
}

void GetDataFromRegistry()
{
	HKEY key;
	DWORD dwEnabled;
	DWORD lpcbData = sizeof(dwEnabled);
	DWORD dwType = 0;
    DWORD dwValueIndex = 0;
	DWORD unreadIndex = 0;
    BYTE pData[256] = {0};
    DWORD dwDataLength = 256;
	DWORD dwNameLength = 64;
	TCHAR szBuffer[64] = {0};

	DWORD max_sub_key_len;
    DWORD max_val_name_len;
    DWORD max_val_size;


	if (RegOpenKeyEx(HKEY_LOCAL_MACHINE,_T("Software\\Apps\\JustForFun PockeTwit\\UnreadCount"),0,0,&key) == ERROR_SUCCESS)
	{

		if (RegQueryInfoKey(key, NULL, NULL, NULL, NULL, &max_sub_key_len, NULL, &unreadGroupsCount, &max_val_name_len, &max_val_size, NULL, NULL )== ERROR_SUCCESS)
		{
			unreadGroupsCount--;

			unreadCountPointer = new UnreadCount[unreadGroupsCount];
			hasUnreadGroups = FALSE;

			while( RegEnumValue(key, dwValueIndex, szBuffer, &dwNameLength, NULL, &dwType, pData, &dwDataLength) == ERROR_SUCCESS )
			{
				switch( dwType )
				{
					case REG_DWORD:
						{
							DWORD dwValue = *((LPDWORD)pData);

							unreadCountPointer[unreadIndex].dwUnread = dwValue;

							wcscpy(unreadCountPointer[unreadIndex].GroupName, szBuffer);
							
							TCHAR sValue[3];
							wcscpy(unreadCountPointer[unreadIndex].swUnread, _ultow(dwValue,sValue,10));

							if (dwValue > 0)
								hasUnreadGroups = TRUE;

							unreadIndex++;

							WCHAR msgbuf[128] = _T("\t%s - %d\n");
							wprintf(msgbuf, szBuffer, dwValue);
						}
						break;
					default:
						
						break;
				}

				//Reset everything for next itteration
				szBuffer[0] = 0;
				dwNameLength = 64;

				pData[0] = 0;
				dwDataLength = 256;

				dwValueIndex++;
			}
		}

		RegCloseKey(key);
	}

}

void StartPockeTwit()
{
	HKEY key;
	TCHAR szInstallDir[256] = {0};
	TCHAR fullAppPath[256] = {0};
	DWORD lpcbData = sizeof(szInstallDir);

	if (RegOpenKeyEx(HKEY_LOCAL_MACHINE,_T("Software\\Apps\\JustForFun PockeTwit"),0,0,&key) == ERROR_SUCCESS)
	{
		if (RegQueryValueEx(key,_T("InstallDir"),0,0,(LPBYTE)&szInstallDir, &lpcbData) == ERROR_SUCCESS)
		{

			wcscat(fullAppPath,szInstallDir);
			wcscat(fullAppPath, L"\\PockeTwit.exe");

			SHELLEXECUTEINFO ShExecInfo;
			memset( &ShExecInfo, 0, sizeof( SHELLEXECUTEINFO ) );
			ShExecInfo.cbSize = sizeof(SHELLEXECUTEINFO);
			ShExecInfo.fMask = SEE_MASK_FLAG_NO_UI;
			ShExecInfo.hwnd = NULL;
			ShExecInfo.lpVerb = L"Open";
			ShExecInfo.lpFile = fullAppPath;        
			ShExecInfo.lpParameters = NULL;  
			ShExecInfo.lpDirectory = szInstallDir;
			ShExecInfo.nShow = SW_SHOWNORMAL;
			ShExecInfo.hInstApp = NULL; 
			ShellExecuteEx(&ShExecInfo);
		}
	}

}

void SelectNextUnreadGroup()
{
	DWORD i;
	for (i = unreadGroupsCurrent; i < unreadGroupsCount; i++)
	{
		if (unreadCountPointer[i].dwUnread != 0)
		{
			unreadGroupsCurrent = i;
			break;
		}
	}

	// HACK
	if (i >= unreadGroupsCount)
	{
		unreadGroupsCurrent = 0;
		for (i = unreadGroupsCurrent; i < unreadGroupsCount; i++)
		{
			if (unreadCountPointer[i].dwUnread != 0)
			{
				unreadGroupsCurrent = i;
				break;
			}
		}
	}
}

void ShowError()
{
	LPVOID lpMsgBuf;

	FormatMessage( 
		FORMAT_MESSAGE_ALLOCATE_BUFFER | 
		FORMAT_MESSAGE_FROM_SYSTEM | 
		FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		GetLastError(),
		0, // Default language
		(LPTSTR) &lpMsgBuf,
		0,
		NULL 
	);
	// Process any inserts in lpMsgBuf.
	// ...
	// Display the string.
	MessageBox( NULL, (LPCTSTR)lpMsgBuf, L"Error", MB_OK | MB_ICONINFORMATION );
	// Free the buffer.
	LocalFree( lpMsgBuf );
}
