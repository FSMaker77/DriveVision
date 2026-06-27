#include "pch.h"
#include <windows.h>
#include <winioctl.h>
#include <string>


// Firma del nostro walkie-talkie
typedef void(__stdcall* FileFoundCallback)(
    unsigned long long id,
    unsigned long long parentId,
    const wchar_t* nome,
    unsigned long long dimensione,
    unsigned char isDirectory
    );


extern "C"
{
    __declspec(dllexport) void __cdecl AvviaScansioneNativa(const wchar_t* letteraDrive, FileFoundCallback inviaDatiAlCSharp)
    {
        if (inviaDatiAlCSharp == nullptr) return;

        // Costruiamo il percorso del disco (es. "\\.\C:")
        std::wstring drivePath = std::wstring(L"\\\\.\\") + letteraDrive;
        if (drivePath.back() == L'\\') {
            drivePath.pop_back(); // Rimuove lo slash finale se l'utente lo ha inserito
        }

        // Apriamo l'accesso fisico al disco
        HANDLE hDrive = CreateFileW(drivePath.c_str(), GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, NULL);
        if (hDrive == INVALID_HANDLE_VALUE) return;

        MFT_ENUM_DATA_V0 mftEnumData;
        mftEnumData.StartFileReferenceNumber = 0;
        mftEnumData.LowUsn = 0;
        mftEnumData.HighUsn = MAXLONGLONG;

        // Allochiamo 1 Megabyte di RAM (una volta sola!) per leggere la MFT a blocchi
        const DWORD bufferSize = 1024 * 1024;
        BYTE* buffer = new BYTE[bufferSize];
        DWORD bytesReturned = 0;

        // Inizia la scansione massiva
        while (DeviceIoControl(hDrive, FSCTL_ENUM_USN_DATA, &mftEnumData, sizeof(mftEnumData), buffer, bufferSize, &bytesReturned, NULL))
        {
            DWORDLONG nextUsn = *((DWORDLONG*)buffer);
            mftEnumData.StartFileReferenceNumber = nextUsn;

            BYTE* pRecord = buffer + sizeof(USN);
            while (pRecord < buffer + bytesReturned)
            {
                DWORD recordLength = *((DWORD*)pRecord);
                if (recordLength == 0) break;

                WORD majorVersion = *((WORD*)(pRecord + 4));
                if (majorVersion == 2 || majorVersion == 3)
                {
                    // Calcoliamo la posizione dei dati dentro i byte grezzi
                    WORD nameOffset = (majorVersion == 2) ? *((WORD*)(pRecord + 58)) : *((WORD*)(pRecord + 74));
                    WORD nameLength = (majorVersion == 2) ? *((WORD*)(pRecord + 56)) : *((WORD*)(pRecord + 72));
                    DWORD fileAttributes = (majorVersion == 2) ? *((DWORD*)(pRecord + 52)) : *((DWORD*)(pRecord + 68));
                    DWORDLONG fileId = *((DWORDLONG*)(pRecord + 8));
                    DWORDLONG parentId = (majorVersion == 2) ? *((DWORDLONG*)(pRecord + 16)) : *((DWORDLONG*)(pRecord + 24));

                    // Estraiamo il nome del file
                    std::wstring fileName(reinterpret_cast<wchar_t*>(pRecord + nameOffset), nameLength / sizeof(wchar_t));

                    unsigned char isDir = (fileAttributes & FILE_ATTRIBUTE_DIRECTORY) ? 1 : 0;

                    // INVIAMO I DATI AL C# IN TEMPO REALE!
                    // Passiamo 0 come dimensione, il C# calcolerà le dimensioni dopo con il Parallel.ForEach
                    inviaDatiAlCSharp(fileId, parentId, fileName.c_str(), 0, isDir);
                }

                pRecord += recordLength;
            }
        }

        // Puliamo la memoria (in C++ bisogna farlo a mano!) e chiudiamo il disco
        delete[] buffer;
        CloseHandle(hDrive);
    }
}