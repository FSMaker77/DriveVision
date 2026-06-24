using System.Collections.Generic;

namespace DriveVision
{
    public static class Translator
    {
        public static string LinguaCorrente { get; private set; } = "IT";

        private static Dictionary<string, Dictionary<string, string>> _dizionari = new Dictionary<string, Dictionary<string, string>>
        {
            { "IT", new Dictionary<string, string> {
                { "MenuFile", "_File" }, { "MenuEsci", "Esci" }, { "MenuRisoluzione", "_Risoluzione" }, { "RisDef", "1920 x 1080 (FHD - Predefinita)" },
                { "MenuInfo", "_Info" }, { "MenuSupportami", "_Supportami" }, { "SelezionaUnita", "Seleziona un'unità per avviare l'analisi" }, { "AggiornaLista", "Aggiorna lista dischi" }, { "CambiaLingua", "Cambia lingua" },
                { "InfoTitolo", "Info su DriveVision" }, 
                
                // NUOVE CHIAVI PER LA FINESTRA INFO
                { "InfoDesc", "Software moderno, open-source e ultra-veloce per la diagnosi dello spazio occupato sui dischi rigidi/supporti esterni." },
                { "InfoSviluppatore", "Sviluppato da Federico Salis" },
                { "InfoSito", "Sito" }, { "InfoBlog", "Blog" }, { "InfoAltro", "Altro" },

                { "ErroreBrowser", "Impossibile aprire automaticamente il browser.\nPuoi supportare il progetto visitando: " },
                { "TotaleDisco", "Spazio Totale Disco: " }, { "Occupato", "Spazio Occupato: " }, { "Libero", "Spazio Libero: " },
                { "NomeFile", "Nome File / Cartella" }, { "Tipo", "Tipo" }, { "Dimensione", "Dimensione" }, { "Impatto", "Impatto Spazio" },
                { "Estensione", "Estensione" }, { "File", "File" }, { "Cartella", "Cartella" },
                { "AnalisiDi", "Analisi di " }, { "AvvioMotore", "Avvio motore" }, { "EstrazioneMFT", "Estrazione MFT e calcolo pesi su " },
                { "UltraVeloce", "Ultra-veloce (MFT + SSD)" }, { "Veloce", "Veloce (MFT + HDD)" },
                { "DiscoLocale", "Disco Locale" }, { "LentoUSB", "Lento (USB)" }, { "StandardNoMFT", "Standard (No MFT)" },
                { "ApriEsplora", "Apri in Esplora Risorse" }, { "Elimina", "Elimina" }, { "EliminaDef", "Elimina definitivamente" },
                { "TornaSu", "Torna alla cartella superiore" }, { "TornaHome", "Torna alla selezione dischi" },
                { "DomandaCestino", "Vuoi spostare" }, { "NelCestino", "nel Cestino?" },
                { "DomandaDefinitiva", "Sei sicuro di voler eliminare DEFINITIVAMENTE" }, { "NonAnnullabile", "\nQuesta operazione non può essere annullata." },
                { "ErroreEli", "Si è verificato un errore durante l'eliminazione.\nVerifica che il file non sia aperto in un altro programma." },
                { "ErroreAdmin", "Errore: Controlla i privilegi di Amministratore." }, { "NonSupportato", "non è supportato (richiesto NTFS)." }
            }},
            { "EN", new Dictionary<string, string> {
                { "MenuFile", "_File" }, { "MenuEsci", "Exit" }, { "MenuRisoluzione", "_Resolution" }, { "RisDef", "1920 x 1080 (FHD - Default)" },
                { "MenuInfo", "_About" }, { "MenuSupportami", "_Support Me" }, { "SelezionaUnita", "Select a drive to start the analysis" }, { "AggiornaLista", "Refresh drives list" }, { "CambiaLingua", "Change language" },
                { "InfoTitolo", "About DriveVision" },

                { "InfoDesc", "A modern, open-source, and ultra-fast software for diagnosing space usage on hard drives/external media." },
                { "InfoSviluppatore", "Developed by Federico Salis" },
                { "InfoSito", "Website" }, { "InfoBlog", "Blog" }, { "InfoAltro", "Other" },

                { "ErroreBrowser", "Unable to open the browser automatically.\nYou can support the project at: " },
                { "TotaleDisco", "Total Disk Space: " }, { "Occupato", "Used Space: " }, { "Libero", "Free Space: " },
                { "NomeFile", "File / Folder Name" }, { "Tipo", "Type" }, { "Dimensione", "Size" }, { "Impatto", "Space Impact" },
                { "Estensione", "Extension" }, { "File", "Files" }, { "Cartella", "Folder" },
                { "AnalisiDi", "Analysis of " }, { "AvvioMotore", "Starting engine" }, { "EstrazioneMFT", "MFT extraction and size calculation on " },
                { "UltraVeloce", "Ultra-fast (MFT + SSD)" }, { "Veloce", "Fast (MFT + HDD)" },
                { "DiscoLocale", "Local Disk" }, { "LentoUSB", "Slow (USB)" }, { "StandardNoMFT", "Standard (No MFT)" },
                { "ApriEsplora", "Open in Explorer" }, { "Elimina", "Delete" }, { "EliminaDef", "Delete permanently" },
                { "TornaSu", "Go to parent folder" }, { "TornaHome", "Return to drive selection" },
                { "DomandaCestino", "Do you want to move" }, { "NelCestino", "to the Recycle Bin?" },
                { "DomandaDefinitiva", "Are you sure you want to PERMANENTLY delete" }, { "NonAnnullabile", "\nThis operation cannot be undone." },
                { "ErroreEli", "An error occurred during deletion.\nCheck that the file is not open in another program." },
                { "ErroreAdmin", "Error: Check Administrator privileges." }, { "NonSupportato", "is not supported (NTFS required)." }
            }},
            { "ES", new Dictionary<string, string> {
                { "MenuFile", "_Archivo" }, { "MenuEsci", "Salir" }, { "MenuRisoluzione", "_Resolución" }, { "RisDef", "1920 x 1080 (FHD - Predeterminada)" },
                { "MenuInfo", "_Info" }, { "MenuSupportami", "_Apóyame" }, { "SelezionaUnita", "Seleccione una unidad para iniciar el análisis" }, { "AggiornaLista", "Actualizar lista de discos" }, { "CambiaLingua", "Cambiar idioma" },
                { "InfoTitolo", "Acerca de DriveVision" },

                { "InfoDesc", "Software moderno, de código abierto y ultrarrápido para el diagnóstico del espacio ocupado en discos duros/soportes externos." },
                { "InfoSviluppatore", "Desarrollado por Federico Salis" },
                { "InfoSito", "Sitio web" }, { "InfoBlog", "Blog" }, { "InfoAltro", "Otro" },

                { "ErroreBrowser", "No se pudo abrir el navegador automáticamente.\nPuedes apoyar el proyecto visitando: " },
                { "TotaleDisco", "Espacio Total: " }, { "Occupato", "Espacio Usado: " }, { "Libero", "Espacio Libre: " },
                { "NomeFile", "Nombre de Archivo / Carpeta" }, { "Tipo", "Tipo" }, { "Dimensione", "Tamaño" }, { "Impatto", "Impacto Espacial" },
                { "Estensione", "Extensión" }, { "File", "Archivos" }, { "Cartella", "Carpeta" },
                { "AnalisiDi", "Análisis de " }, { "AvvioMotore", "Iniciando motor" }, { "EstrazioneMFT", "Extracción MFT y cálculo de peso en " },
                { "UltraVeloce", "Ultrarrápido (MFT + SSD)" }, { "Veloce", "Rápido (MFT + HDD)" },
                { "DiscoLocale", "Disco Local" }, { "LentoUSB", "Lento (USB)" }, { "StandardNoMFT", "Estándar (Sin MFT)" },
                { "ApriEsplora", "Abrir en el Explorador" }, { "Elimina", "Eliminar" }, { "EliminaDef", "Eliminar permanentemente" },
                { "TornaSu", "Ir a la carpeta superior" }, { "TornaHome", "Volver a la selección de discos" },
                { "DomandaCestino", "¿Deseas mover" }, { "NelCestino", "a la Papelera?" },
                { "DomandaDefinitiva", "¿Estás seguro de que deseas eliminar PERMANENTEMENTE" }, { "NonAnnullabile", "\nEsta operación no se puede deshacer." },
                { "ErroreEli", "Ocurrió un error durante la eliminación.\nVerifica que el archivo no esté abierto en otro programa." },
                { "ErroreAdmin", "Error: Verifique los privilegios de Administrador." }, { "NonSupportato", "no es compatible (se requiere NTFS)." }
            }},
            { "FR", new Dictionary<string, string> {
                { "MenuFile", "_Fichier" }, { "MenuEsci", "Quitter" }, { "MenuRisoluzione", "_Résolution" }, { "RisDef", "1920 x 1080 (FHD - Par défaut)" },
                { "MenuInfo", "_À propos" }, { "MenuSupportami", "_Soutenez-moi" }, { "SelezionaUnita", "Sélectionnez un lecteur pour lancer l'analyse" }, { "AggiornaLista", "Actualiser la liste des disques" }, { "CambiaLingua", "Changer de langue" },
                { "InfoTitolo", "À propos de DriveVision" },

                { "InfoDesc", "Logiciel moderne, open-source et ultra-rapide pour diagnostiquer l'espace occupé sur les disques durs/supports externes." },
                { "InfoSviluppatore", "Développé par Federico Salis" },
                { "InfoSito", "Site web" }, { "InfoBlog", "Blog" }, { "InfoAltro", "Autre" },

                { "ErroreBrowser", "Impossible d'ouvrir le navigateur automatiquement.\nVous pouvez soutenir le projet en visitant : " },
                { "TotaleDisco", "Espace Disque Total : " }, { "Occupato", "Espace Utilisé : " }, { "Libero", "Espace Libre : " },
                { "NomeFile", "Nom de Fichier / Dossier" }, { "Tipo", "Type" }, { "Dimensione", "Taille" }, { "Impatto", "Impact Spatial" },
                { "Estensione", "Extension" }, { "File", "Fichiers" }, { "Cartella", "Dossier" },
                { "AnalisiDi", "Analyse de " }, { "AvvioMotore", "Démarrage du moteur" }, { "EstrazioneMFT", "Extraction MFT et calcul de la taille sur " },
                { "UltraVeloce", "Ultra-rapide (MFT + SSD)" }, { "Veloce", "Rapide (MFT + HDD)" },
                { "DiscoLocale", "Disque Local" }, { "LentoUSB", "Lent (USB)" }, { "StandardNoMFT", "Standard (Sans MFT)" },
                { "ApriEsplora", "Ouvrir dans l'Explorateur" }, { "Elimina", "Supprimer" }, { "EliminaDef", "Supprimer définitivement" },
                { "TornaSu", "Aller au dossier parent" }, { "TornaHome", "Retour à la sélection du lecteur" },
                { "DomandaCestino", "Voulez-vous déplacer" }, { "NelCestino", "vers la Corbeille ?" },
                { "DomandaDefinitiva", "Êtes-vous sûr de vouloir supprimer DÉFINITIVEMENT" }, { "NonAnnullabile", "\nCette opération ne peut pas être annulée." },
                { "ErroreEli", "Une erreur s'est produite lors de la suppression.\nVérifiez que le fichier n'est pas ouvert dans un autre programme." },
                { "ErroreAdmin", "Erreur : Vérifiez les privilèges d'Administrateur." }, { "NonSupportato", "n'est pas pris en charge (NTFS requis)." }
            }},
            { "DE", new Dictionary<string, string> {
                { "MenuFile", "_Datei" }, { "MenuEsci", "Beenden" }, { "MenuRisoluzione", "_Auflösung" }, { "RisDef", "1920 x 1080 (FHD - Standard)" },
                { "MenuInfo", "_Info" }, { "MenuSupportami", "_Unterstützen" }, { "SelezionaUnita", "Wählen Sie ein Laufwerk aus, um die Analyse zu starten" }, { "AggiornaLista", "Laufwerksliste aktualisieren" }, { "CambiaLingua", "Sprache ändern" },
                { "InfoTitolo", "Über DriveVision" },

                { "InfoDesc", "Moderne, quelloffene und ultraschnelle Software zur Diagnose der Speicherbelegung auf Festplatten/externen Medien." },
                { "InfoSviluppatore", "Entwickelt von Federico Salis" },
                { "InfoSito", "Webseite" }, { "InfoBlog", "Blog" }, { "InfoAltro", "Weiteres" },

                { "ErroreBrowser", "Browser konnte nicht automatisch geöffnet werden.\nSie können das Projekt unterstützen unter: " },
                { "TotaleDisco", "Gesamtspeicherplatz: " }, { "Occupato", "Belegter Speicher: " }, { "Libero", "Freier Speicher: " },
                { "NomeFile", "Datei- / Ordnername" }, { "Tipo", "Typ" }, { "Dimensione", "Größe" }, { "Impatto", "Speicherauswirkung" },
                { "Estensione", "Erweiterung" }, { "File", "Dateien" }, { "Cartella", "Ordner" },
                { "AnalisiDi", "Analyse von " }, { "AvvioMotore", "Motor starten" }, { "EstrazioneMFT", "MFT-Extraktion und Größenberechnung auf " },
                { "UltraVeloce", "Ultraschnell (MFT + SSD)" }, { "Veloce", "Schnell (MFT + HDD)" },
                { "DiscoLocale", "Lokaler Datenträger" }, { "LentoUSB", "Langsam (USB)" }, { "StandardNoMFT", "Standard (Ohne MFT)" },
                { "ApriEsplora", "Im Explorer öffnen" }, { "Elimina", "Löschen" }, { "EliminaDef", "Dauerhaft löschen" },
                { "TornaSu", "Zum übergeordneten Ordner" }, { "TornaHome", "Zur Laufwerksauswahl zurück" },
                { "DomandaCestino", "Möchten Sie" }, { "NelCestino", "in den Papierkorb verschieben?" },
                { "DomandaDefinitiva", "Sind Sie sicher, dass Sie DAUERHAFT löschen möchten:" }, { "NonAnnullabile", "\nDieser Vorgang kann nicht rückgängig gemacht werden." },
                { "ErroreEli", "Beim Löschen ist ein Fehler aufgetreten.\nÜberprüfen Sie, ob die Datei nicht in einem anderen Programm geöffnet ist." },
                { "ErroreAdmin", "Fehler: Überprüfen Sie die Administratorrechte." }, { "NonSupportato", "wird nicht unterstützt (NTFS erforderlich)." }
            }}
        };

        public static void ImpostaLingua(string codiceLingua)
        {
            if (_dizionari.ContainsKey(codiceLingua))
            {
                LinguaCorrente = codiceLingua;
            }
        }

        public static string Get(string chiave)
        {
            if (_dizionari[LinguaCorrente].TryGetValue(chiave, out string valore))
            {
                return valore;
            }
            return chiave;
        }
    }
}