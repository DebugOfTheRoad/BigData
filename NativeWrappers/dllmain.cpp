// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

extern "C" {
	_declspec(dllexport) BOOL SetTouchDisableProperty(HWND hwnd, BOOL fDisableTouch) {
		IPropertyStore *pPropStore;
		HRESULT hrReturnValue = SHGetPropertyStoreForWindow(hwnd, IID_PPV_ARGS(&pPropStore));
		if (SUCCEEDED(hrReturnValue)) {
			PROPVARIANT var;
			var.vt = VT_BOOL;
			var.boolVal = fDisableTouch ? VARIANT_TRUE : VARIANT_FALSE;
			hrReturnValue = pPropStore->SetValue(PKEY_EdgeGesture_DisableTouchWhenFullscreen, var);
			pPropStore->Release();
		}
		return SUCCEEDED(hrReturnValue);
	}
}

