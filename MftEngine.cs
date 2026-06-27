using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks; // Aggiunto per l'esecuzione Parallela

namespace DriveVision
{
    public class FileNode
    {
        public ulong Id { get; set; }
        public ulong ParentId { get; set; }
        public string Nome { get; set; } = string.Empty;
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
                                ulong fileId = (ulong)Marshal.ReadInt64(pRecord, 8);
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

            // Nuova logica a 3 fasi ad altissime prestazioni
            List<FileNode> listaDiSoliFile = new List<FileNode>();
            AssegnaPercorsiEstraiFile(root, rootPath, listaDiSoliFile);
            RecuperaDimensioniParallelo(listaDiSoliFile);
            CalcolaDimensioniCartelle(root);

            return root;
        }

        private FileNode CostruisciAlbero(Dictionary<ulong, FileNode> nodi, string pathRadice)
        {
            FileNode root = new FileNode { Nome = pathRadice, IsDirectory = true, PercorsoCompleto = pathRadice };
            foreach (var nodo in nodi.Values)
            {
                if (nodi.TryGetValue(nodo.ParentId, out FileNode genitore))
                    genitore.Figli.Add(nodo);
                else
                    root.Figli.Add(nodo);
            }
            return root;
        }

        // FASE 1: Costruisce rapidamente le stringhe dei percorsi e isola solo i file (ignorando le cartelle)
        private void AssegnaPercorsiEstraiFile(FileNode nodo, string percorsoAttuale, List<FileNode> listaFile)
        {
            foreach (var figlio in nodo.Figli)
            {
                figlio.PercorsoCompleto = percorsoAttuale + figlio.Nome;

                if (figlio.IsDirectory)
                {
                    figlio.PercorsoCompleto += "\\"; // Aggiunge lo slash finale per le cartelle
                    AssegnaPercorsiEstraiFile(figlio, figlio.PercorsoCompleto, listaFile);
                }
                else
                {
                    listaFile.Add(figlio); // Mette da parte il file per la scansione parallela
                }
            }
        }

        // FASE 2: Interroga l'SSD usando tutti i core del processore contemporaneamente (Speedup enorme!)
        private void RecuperaDimensioniParallelo(List<FileNode> listaFile)
        {
            Parallel.ForEach(listaFile, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, file =>
            {
                if (GetFileAttributesEx(file.PercorsoCompleto, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out WIN32_FILE_ATTRIBUTE_DATA info))
                {
                    file.DimensioneByte = ((long)info.nFileSizeHigh << 32) | info.nFileSizeLow;
                }
            });
        }

        // FASE 3: Risale l'albero per sommare le dimensioni all'interno delle cartelle genitore
        private void CalcolaDimensioniCartelle(FileNode nodo)
        {
            long dimensioneTotale = 0;

            foreach (var figlio in nodo.Figli)
            {
                if (figlio.IsDirectory)
                {
                    CalcolaDimensioniCartelle(figlio);
                }
                dimensioneTotale += figlio.DimensioneByte;
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