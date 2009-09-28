#include "snapi.h"
#include "Regext.h"

#define IDT_TIMER_NEXT_GROUP 10010

HREGNOTIFY RegisterUnreadCountChangedCallback();
void UnreadCountChangedCallback(HREGNOTIFY hNotify, DWORD dwUserData, const PBYTE pData, const UINT cbData);
void GetDataFromRegistry();
void StartPockeTwit();
void SelectNextUnreadGroup();
void ShowError();

struct UnreadCount
{ TCHAR GroupName[64];
  TCHAR swUnread[3];
  DWORD dwUnread;
};