using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
        // Importiamo la funzione nativa dalla nostra DLL
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void FileFoundDelegate(ulong id, ulong parentId, string nome, long dimensione, byte isDirectory);

        [DllImport("MftNativeEngine.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void AvviaScansioneNativa(string letteraDrive, FileFoundDelegate callback);

        public FileNode? AvviaScansione(string letteraDrive)
        {
            var tuttiINodi = new Dictionary<ulong, FileNode>();
            string baseDrive = letteraDrive.TrimEnd('\\');
            string rootPath = baseDrive + "\\";

            // 1. Definiamo come gestire ogni file che arriva dal C++
            FileFoundDelegate ricevitoreDati = (id, parentId, nome, dimensione, isDirectory) =>
            {
                var node = new FileNode
                {
                    Id = id,
                    ParentId = parentId,
                    Nome = nome,
                    IsDirectory = (isDirectory == 1),
                    DimensioneByte = 0
                };
                tuttiINodi[id] = node;
            };

            // 2. Chiamiamo il motore C++ (la scansione ora dura una frazione di secondo!)
            try
            {
                AvviaScansioneNativa(baseDrive, ricevitoreDati);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante scansione nativa: {ex.Message}");
                return null;
            }

            // 3. Costruiamo l'albero e calcoliamo le dimensioni (tutto in C# come prima)
            FileNode root = CostruisciAlbero(tuttiINodi, rootPath);

            List<FileNode> listaDiSoliFile = new List<FileNode>();
            AssegnaPercorsiEstraiFile(root, rootPath, listaDiSoliFile);

            // Usiamo il tuo metodo parallelo super veloce
            RecuperaDimensioniParallelo(listaDiSoliFile);
            CalcolaDimensioniCartelle(root);

            return root;
        }

        private FileNode CostruisciAlbero(Dictionary<ulong, FileNode> nodi, string pathRadice)
        {
            FileNode root = new FileNode { Nome = pathRadice, IsDirectory = true, PercorsoCompleto = pathRadice };
            foreach (var nodo in nodi.Values)
            {
                if (nodi.TryGetValue(nodo.ParentId, out FileNode? genitore))
                    genitore.Figli.Add(nodo);
                else
                    root.Figli.Add(nodo);
            }
            return root;
        }

        private void AssegnaPercorsiEstraiFile(FileNode nodo, string percorsoAttuale, List<FileNode> listaFile)
        {
            foreach (var figlio in nodo.Figli)
            {
                figlio.PercorsoCompleto = percorsoAttuale + figlio.Nome;
                if (figlio.IsDirectory)
                {
                    figlio.PercorsoCompleto += "\\";
                    AssegnaPercorsiEstraiFile(figlio, figlio.PercorsoCompleto, listaFile);
                }
                else
                {
                    listaFile.Add(figlio);
                }
            }
        }

        private void RecuperaDimensioniParallelo(List<FileNode> listaFile)
        {
            Parallel.ForEach(listaFile, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, file =>
            {
                // Ottiene la dimensione reale dei file interrogando l'SSD in parallelo
                var fi = new System.IO.FileInfo(file.PercorsoCompleto);
                try { file.DimensioneByte = fi.Length; } catch { file.DimensioneByte = 0; }
            });
        }

        private void CalcolaDimensioniCartelle(FileNode nodo)
        {
            long dimensioneTotale = 0;
            foreach (var figlio in nodo.Figli)
            {
                if (figlio.IsDirectory) CalcolaDimensioniCartelle(figlio);
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