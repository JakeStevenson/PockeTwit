#include "snapi.h"
#include "Regext.h"

#define IDT_TIMER_NEXT_GROUP 10010

HREGNOTIFY RegisterUnreadCountChangedCallback();
void UnreadCountChangedCallback(HREGNOTIFY hNotify, DWORD dwUserData, const PBYTE pData, const UINT cbData);
void UpdateData();
void GetDataFromRegistry();
void StartPockeTwit();
void SelectNextUnreadGroup();
void ShowError();
void ToggleMode(BOOL Compact);
void ToggleStartDebugBuild(BOOL enabled);

struct UnreadCount
{ TCHAR GroupName[64];
  TCHAR swUnread[3];
  DWORD dwUnread;
};