using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DriveVision
{
    public class FileNode
    {
        public ulong Id { get; set; }
        public ulong ParentId { get; set; }
        public string Nome { get; set; } = string.Empty;

        // NUOVO: Necessario per aprire ed eliminare i file
        public string PercorsoCompleto { get; set; } = string.Empty;

        public long DimensioneByte { get; set; }
        public double Percentuale { get; set; }
        public bool IsDirectory { get; set; }
        public List<FileNode> Figli { get; set; } = new List<FileNode>();

        public string Tipo => IsDirectory ? Translator.Get("Cartella") : Translator.Get("File");
        public string DimensioneFormattata
        {
            get
            {
                if (DimensioneByte == 0) return "0 B";
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                int order = 0;
                double len = DimensioneByte;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }
    }

    public class MftEngine
    {
        private const uint GENERIC_READ = 0x80000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint OPEN_EXISTING = 3;
        private const uint FSCTL_ENUM_USN_DATA = 0x000900B3;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, int nInBufferSize, IntPtr lpOutBuffer, int nOutBufferSize, out int lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private enum GET_FILEEX_INFO_LEVELS { GetFileExInfoStandard = 0 }

        [StructLayout(LayoutKind.Sequential)]
        private struct WIN32_FILE_ATTRIBUTE_DATA
        {
            public uint dwFileAttributes;
            public uint ftCreationTimeLow;
            public uint ftCreationTimeHigh;
            public uint ftLastAccessTimeLow;
            public uint ftLastAccessTimeHigh;
            public uint ftLastWriteTimeLow;
            public uint ftLastWriteTimeHigh;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool GetFileAttributesEx(string lpFileName, GET_FILEEX_INFO_LEVELS fInfoLevelId, out WIN32_FILE_ATTRIBUTE_DATA fileData);


        [StructLayout(LayoutKind.Sequential)]
        private struct MFT_ENUM_DATA_V0
        {
            public ulong StartFileReferenceNumber;
            public long LowUsn;
            public long HighUsn;
        }

        public FileNode AvviaScansione(string letteraDrive)
        {
            var tuttiINodi = new Dictionary<ulong, FileNode>();
            string baseDrive = letteraDrive.TrimEnd('\\');
            string drivePath = @"\\.\" + baseDrive;
            IntPtr hDrive = CreateFile(drivePath, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

            if (hDrive == IntPtr.Zero || hDrive.ToInt64() == -1) return null;

            try
            {
                MFT_ENUM_DATA_V0 mftEnumData = new MFT_ENUM_DATA_V0 { StartFileReferenceNumber = 0, LowUsn = 0, HighUsn = long.MaxValue };
                int mftEnumDataSize = Marshal.SizeOf(mftEnumData);
                IntPtr pEnumData = Marshal.AllocHGlobal(mftEnumDataSize);
                Marshal.StructureToPtr(mftEnumData, pEnumData, false);

                int bufferSize = 1024 * 1024;
                IntPtr pBuffer = Marshal.AllocHGlobal(bufferSize);

                try
                {
                    while (true)
                    {
                        if (!DeviceIoControl(hDrive, FSCTL_ENUM_USN_DATA, pEnumData, mftEnumDataSize, pBuffer, bufferSize, out int bytesReturned, IntPtr.Zero)) break;
                        ulong nextUsn = (ulong)Marshal.ReadInt64(pBuffer);
                        Marshal.WriteInt64(pEnumData, (long)nextUsn);
                        IntPtr pRecord = new IntPtr(pBuffer.ToInt64() + 8);
                        int offset = 8;

                        while (offset < bytesReturned)
                        {
                            int recordLength = Marshal.ReadInt32(pRecord);
                            if (recordLength == 0) break;

                            short majorVersion = Marshal.ReadInt16(pRecord, 4);
                            if (majorVersion == 2 || majorVersion == 3)
                            {
                                int nameOffset = majorVersion == 2 ? Marshal.ReadInt16(pRecord, 58) : Marshal.ReadInt16(pRecord, 74);
                                int nameLength = majorVersion == 2 ? Marshal.ReadInt16(pRecord, 56) : Marshal.ReadInt16(pRecord, 72);
                                uint fileAttributes = majorVersion == 2 ? (uint)Marshal.ReadInt32(pRecord, 52) : (uint)Marshal.ReadInt32(pRecord, 68);
                                ulong fileId = majorVersion == 2 ? (ulong)Marshal.ReadInt64(pRecord, 8) : (ulong)Marshal.ReadInt64(pRecord, 8);
                                ulong parentId = majorVersion == 2 ? (ulong)Marshal.ReadInt64(pRecord, 16) : (ulong)Marshal.ReadInt64(pRecord, 24);

                                IntPtr pName = new IntPtr(pRecord.ToInt64() + nameOffset);
                                string name = Marshal.PtrToStringUni(pName, nameLength / 2) ?? string.Empty;

                                var node = new FileNode
                                {
                                    Id = fileId,
                                    ParentId = parentId,
                                    Nome = name,
                                    IsDirectory = (fileAttributes & 0x00000010) != 0,
                                    DimensioneByte = 0
                                };
                                tuttiINodi[node.Id] = node;
                            }
                            pRecord = new IntPtr(pRecord.ToInt64() + recordLength);
                            offset += recordLength;
                        }
                    }
                }
                finally { Marshal.FreeHGlobal(pBuffer); Marshal.FreeHGlobal(pEnumData); }
            }
            finally { CloseHandle(hDrive); }

            string rootPath = baseDrive + "\\";
            FileNode root = CostruisciAlbero(tuttiINodi, rootPath);
            PopolaDimensioni(root, rootPath);

            return root;
        }

        private FileNode CostruisciAlbero(Dictionary<ulong, FileNode> nodi, string pathRadice)
        {
            // NUOVO: Assegniamo il percorso alla radice
            FileNode root = new FileNode { Nome = pathRadice, IsDirectory = true, PercorsoCompleto = pathRadice };
            foreach (var nodo in nodi.Values)
            {
                if (nodi.TryGetValue(nodo.ParentId, out FileNode genitore)) genitore.Figli.Add(nodo);
                else root.Figli.Add(nodo);
            }
            return root;
        }

        private void PopolaDimensioni(FileNode nodo, string percorsoAttuale)
        {
            long dimensioneTotale = 0;
            foreach (var figlio in nodo.Figli)
            {
                string percorsoFiglio = percorsoAttuale + figlio.Nome;

                // NUOVO: Salviamo il percorso completo in ogni file per permetterne l'eliminazione
                figlio.PercorsoCompleto = percorsoFiglio;

                if (figlio.IsDirectory)
                {
                    PopolaDimensioni(figlio, percorsoFiglio + "\\");
                    dimensioneTotale += figlio.DimensioneByte;
                }
                else
                {
                    if (GetFileAttributesEx(percorsoFiglio, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out WIN32_FILE_ATTRIBUTE_DATA info))
                    {
                        long dim = ((long)info.nFileSizeHigh << 32) | info.nFileSizeLow;
                        figlio.DimensioneByte = dim;
                        dimensioneTotale += dim;
                    }
                }
            }
            nodo.DimensioneByte = dimensioneTotale;

            if (dimensioneTotale > 0)
            {
                foreach (var figlio in nodo.Figli)
                {
                    figlio.Percentuale = ((double)figlio.DimensioneByte / dimensioneTotale) * 100.0;
                }
            }
        }
    }
}