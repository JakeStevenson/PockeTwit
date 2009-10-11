
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
#define TEXT_GROUP_MARGIN  SCALEY(5)

HWND hwnd;

TCHAR g_szPluginClass[] = TEXT("PockeTwitTodayPlugin");
DWORD g_cyItemHeight;
DWORD g_cyInitItemHeight;
uint g_uMetricChangeMsg2;
DWORD g_cyDefaultItemHeight;

BOOL VGA = FALSE;
BOOL compact = FALSE;
HICON g_hIcon =  NULL;

BOOL startDebugBuild = FALSE;

HREGNOTIFY hrUnreadCountChanged = NULL;

UnreadCount *unreadCountPointer;
DWORD totalGroupsCount,unreadGroupsCount,g_nSelectedGroup;
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

void Plugin_OnPaintCompact(HWND hwnd, HDC hdc)
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
    
    if (hasUnreadGroups)
	{
		lf.lfWeight = FW_BOLD;
	}
	else
	{
		lf.lfWeight = FW_NORMAL;
	}
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


void Plugin_OnPaintFull(HWND hwnd, HDC hdc)
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
	
    if (!SendMessage(hwndParent, TODAYM_DRAWWATERMARK, 0, (LPARAM) &dwi))
    {
        FillRectClr(hdcScreenBuffer, &rcDraw, GetSysColor(COLOR_WINDOW));
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
   
    hSysFont = (HFONT) GetStockObject(SYSTEM_FONT);
    GetObject(hSysFont, sizeof(LOGFONT), &lf);
    
	if (hasUnreadGroups)
	{
		lf.lfWeight = FW_BOLD;
	}
	else
	{
		lf.lfWeight = FW_NORMAL;
	}

    // Calculate the font size, making sure to round the result to the nearest integer
    lf.lfHeight = (long) -((8.0 * (double)GetDeviceCaps(hdcScreenBuffer, LOGPIXELSY) / 72.0)+.5);
    
    // create the font
    hFont = CreateFontIndirect(&lf);
    
    // Select the system font into the device context
    hFontOld = (HFONT) SelectObject(hdcScreenBuffer, hFont);

	if (hasUnreadGroups)
	{
		DWORD i,j;
		j=0;
		for (i = 0; i < totalGroupsCount; i++)
		{
			if (unreadCountPointer[i].dwUnread != 0)
			{
				if (unreadGroupsCount > 1)
				{
					rcText.top = ((long)((8.0 * (double)DRA::GetScreenCaps(LOGPIXELSY) / 72.0)+.5) + TEXT_GROUP_MARGIN ) * j;
					j++;
					rcText.bottom = ((long)((8.0 * (double)DRA::GetScreenCaps(LOGPIXELSY) / 72.0)+.5) + TEXT_GROUP_MARGIN ) * j;

					if (IsItemState(hwnd, PLUGIN_SELECTED) && ((j-1) == g_nSelectedGroup))
					{
						COLORREF crHighlight;

						crHighlight = SendMessage(hwndParent, TODAYM_GETCOLOR, TODAYCOLOR_HIGHLIGHT, 0);
				        
						nBkMode = SetBkMode(hdcScreenBuffer, OPAQUE);
						FillRectClr(hdcScreenBuffer, &rcText, crHighlight);
						SetBkMode(hdcScreenBuffer, nBkMode);
					}
				}
				else
				{
					if (IsItemState(hwnd, PLUGIN_SELECTED))
					{
						COLORREF crHighlight;

						crHighlight = SendMessage(hwndParent, TODAYM_GETCOLOR, TODAYCOLOR_HIGHLIGHT, 0);
				        
						nBkMode = SetBkMode(hdcScreenBuffer, OPAQUE);
						FillRectClr(hdcScreenBuffer, &rcDraw, crHighlight);
						SetBkMode(hdcScreenBuffer, nBkMode);
					}
				}
				
				WCHAR msgbuf[128] = _T("");
				wcscat(msgbuf, unreadCountPointer[i].GroupName);
				wcscat(msgbuf, _T(" ("));
				wcscat(msgbuf, unreadCountPointer[i].swUnread);
				wcscat(msgbuf, _T(")"));
				DrawText(hdcScreenBuffer, msgbuf, -1, &rcText, DT_VCENTER | DT_LEFT | DT_SINGLELINE | DT_NOPREFIX );

			}
		}
	}
	else
	{
		if (IsItemState(hwnd, PLUGIN_SELECTED))
		{
			COLORREF crHighlight;

			crHighlight = SendMessage(hwndParent, TODAYM_GETCOLOR, TODAYCOLOR_HIGHLIGHT, 0);
	        
			nBkMode = SetBkMode(hdcScreenBuffer, OPAQUE);
			FillRectClr(hdcScreenBuffer, &rcDraw, crHighlight);
			SetBkMode(hdcScreenBuffer, nBkMode);
		}

		LPCTSTR pszText = (LPCTSTR) LoadString(g_hinstDLL, IDS_PLUGIN_TEXT, NULL, 0);
		DrawText(hdcScreenBuffer, pszText, -1, &rcText, DT_VCENTER | DT_LEFT | DT_SINGLELINE | DT_NOPREFIX );
	}

	DrawIcon(hdcScreenBuffer, TEXT_LEFT_MARGIN, SCALEY(2), g_hIcon);

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
        //InvalidateRect(hwnd, NULL, FALSE);
        return TRUE;
    }
    else if (IsItemState(hwnd, PLUGIN_TEXTRESIZE))
    {
		ptli->cyp = g_cyItemHeight;
        SetItemState(hwnd, PLUGIN_TEXTRESIZE, FALSE);
        //InvalidateRect(hwnd, NULL, FALSE);
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
			g_nSelectedGroup = unreadGroupsCount - 1;
			SetItemState(hwnd, PLUGIN_SELECTED, TRUE);
            fRet = TRUE;
            break;
        case VK_DOWN:
			g_nSelectedGroup = 0;
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
	
	WCHAR msgbuf[128] = _T("PluginWndProc - %d\n");
	wprintf(msgbuf, uMsg);

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
			if (compact)
			{
				Plugin_OnPaintCompact(hwnd, hDC);
			}
			else
			{
				Plugin_OnPaintFull(hwnd, hDC);
			}
            EndPaint(hwnd, &ps);
            break;            
        }

        case WM_LBUTTONDOWN:
            PostMessage(GetParent(hwnd), TODAYM_TOOKSELECTION, (WPARAM)hwnd, 0);
            SetItemState(hwnd, PLUGIN_SELECTED, TRUE);
			if (compact == FALSE)
			{
				if (unreadGroupsCount > 1)
				{
					POINT point;
					point.x = LOWORD(lParam);
					point.y = HIWORD(lParam);

					g_nSelectedGroup = point.y / ((long)((8.0 * (double)DRA::GetScreenCaps(LOGPIXELSY) / 72.0)+.5) + TEXT_GROUP_MARGIN );
				}
				else
				{
					g_nSelectedGroup = 0;
				}
			}
			InvalidateRect(hwnd, NULL, FALSE);
            break;
		
		case WM_TODAYCUSTOM_USERNAVIGATION:
			if (compact == FALSE)
			{
				InvalidateRect(hwnd, NULL, FALSE);

				if (wParam == VK_UP)   g_nSelectedGroup--;
				if (wParam == VK_DOWN) g_nSelectedGroup++;

				if (g_nSelectedGroup < 0 || g_nSelectedGroup >= unreadGroupsCount)
				{
					lRet = FALSE; // go to the next plug-in
				}
				else
				{
					lRet = TRUE;  // stay in this plug-in
				}
			}
			else
			{
				lRet = DefWindowProc(hwnd, uMsg, wParam, lParam);
			}
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
					if (unreadGroupsCurrent >= totalGroupsCount)
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
                          CW_USEDEFAULT,CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT,
                          hwndParent, NULL, g_hinstDLL, NULL);

	hrUnreadCountChanged = RegisterUnreadCountChangedCallback();

	DWORD useCompact, useDebug; 
    
	RegistryGetDWORD(HKEY_LOCAL_MACHINE, TEXT("Software\\Microsoft\\Today\\Items\\PockeTwit"), TEXT("UseCompactTodayPlugin"), &useCompact);
	compact = useCompact;

	RegistryGetDWORD(HKEY_LOCAL_MACHINE, TEXT("Software\\Microsoft\\Today\\Items\\PockeTwit"), TEXT("StartDebugBuild"), &useDebug);
	startDebugBuild = useDebug;

	GetDataFromRegistry();

	if (compact == FALSE)
	{
		RECT rc;
		GetClientRect(hwnd, &rc);

		g_cyItemHeight = g_cyDefaultItemHeight;

		if (unreadGroupsCount > 1)
		{
			g_cyItemHeight = ((long)((8.0 * (double)DRA::GetScreenCaps(LOGPIXELSY) / 72.0)+.5) + TEXT_GROUP_MARGIN ) * unreadGroupsCount;
		}

		//SetRect(&rc, rc.left, rc.top, rc.right, g_cyItemHeight);
		//MoveWindow(hwnd, rc.left, rc.top, rc.right-rc.left, rc.bottom-rc.top, FALSE);
	}

	ptli->cyp = g_cyItemHeight;

    //display the window
    if(ptli->fEnabled)
    {
        ShowWindow (hwnd, SW_SHOW);
		
		if (compact)
			SetTimer(hwnd,IDT_TIMER_NEXT_GROUP, 5000 ,NULL);
    }

    return hwnd;

}


BOOL APIENTRY CustomItemOptionsDlgProc(HWND hDlg, UINT message, UINT wParam, LONG lParam)
{
	SHINITDLGINFO shidi;
	HWND hDlgItem;

	switch (message) 
	{
		case WM_INITDIALOG:

		DWORD useCompact, useDebug; 
    
		RegistryGetDWORD(HKEY_LOCAL_MACHINE, TEXT("Software\\Microsoft\\Today\\Items\\PockeTwit"), TEXT("UseCompactTodayPlugin"), &useCompact);
		compact = useCompact;

		RegistryGetDWORD(HKEY_LOCAL_MACHINE, TEXT("Software\\Microsoft\\Today\\Items\\PockeTwit"), TEXT("StartDebugBuild"), &useDebug);
		startDebugBuild = useDebug;
	
		shidi.dwMask = SHIDIM_FLAGS;
		shidi.dwFlags = SHIDIF_DONEBUTTON | SHIDIF_SIPDOWN | SHIDIF_SIZEDLGFULLSCREEN;
		shidi.hDlg = hDlg;
		SHInitDialog(&shidi);

		CheckRadioButton(hDlg,IDC_RADIO_COMPACT,IDC_RADIO_FULL, (compact == TRUE) ? IDC_RADIO_COMPACT : IDC_RADIO_FULL );

		hDlgItem = GetDlgItem(hDlg, IDC_CHECK_DEBUGBUILD);
		SendMessage(hDlgItem, BM_SETCHECK, (startDebugBuild == TRUE) ? BST_CHECKED : BST_UNCHECKED, 0);
		
		return TRUE; 

		case WM_COMMAND:
			if (LOWORD(wParam) == IDOK) 
			{
				hDlgItem = GetDlgItem(hDlg, IDC_RADIO_COMPACT);
				if(BST_CHECKED == SendMessage(hDlgItem, BM_GETCHECK, NULL, NULL))
				{
					ToggleMode(TRUE);
				}
				else
				{
					ToggleMode(FALSE);
				}

				hDlgItem = GetDlgItem(hDlg, IDC_CHECK_DEBUGBUILD);
				if(BST_CHECKED == SendMessage(hDlgItem, BM_GETCHECK, NULL, NULL))
				{
					ToggleStartDebugBuild(TRUE);
				}
				else
				{
					ToggleStartDebugBuild(FALSE);
				}

				EndDialog(hDlg, LOWORD(wParam));
				return TRUE;
			}
		break;
		//case WM_CTLCOLORSTATIC:
		//      break;
		default:
		  return DefWindowProc(hDlg, message, wParam, lParam);
	}
	return 0;
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
	UpdateData();
}

void UpdateData()
{
	KillTimer(hwnd, IDT_TIMER_NEXT_GROUP);
	
	GetDataFromRegistry();
	
	g_nSelectedGroup = 0;

	if (compact)
	{
		SelectNextUnreadGroup();
	
		SetTimer(hwnd,IDT_TIMER_NEXT_GROUP, 5000 ,NULL);
	}
	else
	{
		RECT rc;
		GetClientRect(hwnd, &rc);

		g_cyItemHeight = g_cyDefaultItemHeight;

		if (unreadGroupsCount > 1)
		{
			g_cyItemHeight = ((long)((8.0 * (double)DRA::GetScreenCaps(LOGPIXELSY) / 72.0)+.5) + TEXT_GROUP_MARGIN ) * unreadGroupsCount;
		}
	}
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

		if (RegQueryInfoKey(key, NULL, NULL, NULL, NULL, &max_sub_key_len, NULL, &totalGroupsCount, &max_val_name_len, &max_val_size, NULL, NULL )== ERROR_SUCCESS)
		{
			totalGroupsCount--;
			unreadGroupsCount = 0;

			unreadCountPointer = new UnreadCount[totalGroupsCount];
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
							{
								hasUnreadGroups = TRUE;
								unreadGroupsCount++;
							}

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
	TCHAR args[64] = {0};
	if (hasUnreadGroups)
	{

		wcscat(args, L"/Group=");

		if (compact)
		{
			wcscat(args, unreadCountPointer[unreadGroupsCurrent].GroupName);
		}
		else
		{
			DWORD i,j;
			j=0;
			for (i = 0; i < totalGroupsCount; i++)
			{
				if (unreadCountPointer[i].dwUnread != 0)
				{
					if (j == g_nSelectedGroup)
					{
						wcscat(args, unreadCountPointer[i].GroupName);
						break;
					}

					j++;
				}
			}
		}
	}
	else
	{
		wcscat(args,  L"");
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
		HKEY key;
		TCHAR szInstallDir[256] = {0};
		TCHAR fullAppPath[256] = {0};
		DWORD lpcbData = sizeof(szInstallDir);
		LPCWSTR path;

		if (startDebugBuild == TRUE)
		{
			path = _T("Software\\Apps\\JustForFun PockeTwit Dev Build");
		}
		else
		{
			path = _T("Software\\Apps\\JustForFun PockeTwit");
		}

		if (RegOpenKeyEx(HKEY_LOCAL_MACHINE,path,0,0,&key) == ERROR_SUCCESS)
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
				ShExecInfo.lpParameters = args;  
				ShExecInfo.lpDirectory = szInstallDir;
				ShExecInfo.nShow = SW_SHOWNORMAL;
				ShExecInfo.hInstApp = NULL; 
				ShellExecuteEx(&ShExecInfo);
			}
		}
	}
}

void SelectNextUnreadGroup()
{
	DWORD i;
	for (i = unreadGroupsCurrent; i < totalGroupsCount; i++)
	{
		if (unreadCountPointer[i].dwUnread != 0)
		{
			unreadGroupsCurrent = i;
			break;
		}
	}

	// HACK
	if (i >= totalGroupsCount)
	{
		unreadGroupsCurrent = 0;
		for (i = unreadGroupsCurrent; i < totalGroupsCount; i++)
		{
			if (unreadCountPointer[i].dwUnread != 0)
			{
				unreadGroupsCurrent = i;
				break;
			}
		}
	}
}

void ToggleMode(BOOL Compact)
{
	compact = Compact;
	RegistrySetDWORD(HKEY_LOCAL_MACHINE, TEXT("Software\\Microsoft\\Today\\Items\\PockeTwit"), TEXT("UseCompactTodayPlugin"), compact);
	if (g_hinstDLL != NULL)
		UpdateData();
}

void ToggleStartDebugBuild(BOOL enabled)
{
	if (enabled == TRUE)
	{
		DWORD useDebug;
		if (ERROR_SUCCESS == RegistryGetDWORD(HKEY_LOCAL_MACHINE, TEXT("Software\\Apps\\JustForFun PockeTwit Dev Build"), TEXT("Instl"), &useDebug))
		{
			startDebugBuild = TRUE;
		}
		else
		{
			startDebugBuild = FALSE;
		}
	}
	else
	{
		startDebugBuild = FALSE;
	}

	RegistrySetDWORD(HKEY_LOCAL_MACHINE, TEXT("Software\\Microsoft\\Today\\Items\\PockeTwit"), TEXT("StartDebugBuild"), startDebugBuild);
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
