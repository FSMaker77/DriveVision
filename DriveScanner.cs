using System;
using System.IO;
using System.Management;
using System.Collections.Generic;
using System.Diagnostics;

namespace DriveVision
{
    public class DriveCard
    {
        public string Lettera { get; set; } = string.Empty;
        public string NomeVolume { get; set; } = string.Empty;
        public string FileSystem { get; set; } = string.Empty;
        public string Icona { get; set; } = string.Empty;
        public string DescrizioneMotore { get; set; } = string.Empty;
        public double SpazioTotaleGB { get; set; }
    }

    public class DriveScanner
    {
        public List<DriveCard> OttieniDischiDisponibili()
        {
            var listaCard = new List<DriveCard>();
            DriveInfo[] dischi = DriveInfo.GetDrives();

            foreach (DriveInfo disco in dischi)
            {
                if (!disco.IsReady) continue;

                var card = new DriveCard
                {
                    Lettera = disco.Name,
                    NomeVolume = string.IsNullOrWhiteSpace(disco.VolumeLabel) ? Translator.Get("DiscoLocale") : disco.VolumeLabel,
                    FileSystem = disco.DriveFormat,
                    SpazioTotaleGB = Math.Round(disco.TotalSize / 1073741824.0, 1)
                };

                if (disco.DriveType == DriveType.Removable)
                {
                    card.Icona = "🐢";
                    card.DescrizioneMotore = Translator.Get("LentoUSB");
                }
                else if (disco.DriveType == DriveType.Fixed)
                {
                    if (disco.DriveFormat == "NTFS")
                    {
                        bool isSsd = ControllaSeSsd(disco.Name);
                        if (isSsd)
                        {
                            card.Icona = "🚀";
                            card.DescrizioneMotore = Translator.Get("UltraVeloce");
                        }
                        else
                        {
                            card.Icona = "🐇";
                            card.DescrizioneMotore = Translator.Get("Veloce");
                        }
                    }
                    else
                    {
                        card.Icona = "🐢";
                        card.DescrizioneMotore = Translator.Get("StandardNoMFT");
                    }
                }
                listaCard.Add(card);
            }
            return listaCard;
        }

        private bool ControllaSeSsd(string letteraDrive)
        {
            try
            {
                string lettera = letteraDrive.Substring(0, 1);

                var partitionSearcher = new ManagementObjectSearcher(
                    @"Root\Microsoft\Windows\Storage",
                    $"SELECT DiskNumber FROM MSFT_Partition WHERE DriveLetter = '{lettera}'");

                foreach (ManagementObject partition in partitionSearcher.Get())
                {
                    if (partition["DiskNumber"] != null)
                    {
                        string diskNumber = partition["DiskNumber"].ToString() ?? string.Empty;

                        var diskSearcher = new ManagementObjectSearcher(
                            @"Root\Microsoft\Windows\Storage",
                            $"SELECT MediaType FROM MSFT_PhysicalDisk WHERE DeviceId = '{diskNumber}'");

                        foreach (ManagementObject disk in diskSearcher.Get())
                        {
                            if (disk["MediaType"] != null && (disk["MediaType"].ToString() ?? string.Empty) == "4")
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore WMI MediaType per {letteraDrive}: {ex.Message}");
            }

            return false;
        }
    }
}