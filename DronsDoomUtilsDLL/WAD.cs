using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DronDoomTexUtilsDLL
{
    public class WAD : IDisposable
    {
        // Variables
        private bool isDisposed = false;

        private string _fileName = "";
        private string _filePath = "";
        private Stream _fileStream = null;
        private BinaryReader _reader = null;
        private Logger _logger = null;

        private bool _header_isIWAD = false;
        private int _header_lumpCount = 0;
        private int _header_directoryOffset = 0;

        private List<Lump> _lumps = null;
        private List<TextureData> _textureData = null;
        private List<string> _patchNames = null;



        // Properties
        public string FileName { get { return _fileName; } }
        public string FilePath { get { return _filePath; } }



        // Constructors
        public WAD(Logger logger = null)
        {
            if (logger != null) _logger = logger;
        }
        public WAD(string wadPath, Logger logger = null)
        {
            if (logger != null) _logger = logger;

            Open(wadPath);
        }



        // Methods
        public bool Open(string wadPath)
        {
            _fileName = System.IO.Path.GetFileName(wadPath);
            _filePath = wadPath;
            _logger?.LogFormat("[{0}] Opening wad...", _fileName);

            if (_fileStream != null)
            {
                _logger?.LogFormat("[{0}] WAD already opened.", _fileName);
                return false;
            }

            if (!File.Exists(wadPath))
            {
                _logger?.LogFormat("[{0}] File doesn't exist!", _fileName);
                return false;
            }

            try
            {
                _fileStream = File.OpenRead(wadPath);
                _reader = new BinaryReader(_fileStream, Encoding.ASCII);
            }
            catch(IOException)
            {
                _logger?.LogFormat("[{0}] Failed to open WAD.", _fileName);
                return false;
            }

            ReadHeader();
            ReadDirectory();
            ReadTextures();

            _logger?.LogFormat("[{0}] WAD successfully opened.", _fileName);
            return true;
        }

        public bool Close()
        {
            _logger?.LogFormat("[{0}] Closing wad...", _fileName);

            if (_fileStream == null)
            {
                _logger?.LogFormat("WAD still not opened.");
                return false;
            }

            if (_lumps != null) foreach (Lump lump in _lumps) lump.Dispose();
            _lumps = null;

            _reader?.Close();
            _reader = null;

            _fileStream.Close();
            _fileStream = null;

            _logger?.LogFormat("[{0}] WAD successfully closed.", _fileName);
            _fileName = "";
            _filePath = "";
            return true;
        }

        private string ReadString(int count) => Encoding.ASCII.GetString(_reader.ReadBytes(count)).Trim('\0');

        private void ReadHeader()
        {
            _fileStream.Seek(0, SeekOrigin.Begin);

            _header_isIWAD = ReadString(4) == "IWAD";
            _header_lumpCount = _reader.ReadInt32();
            _header_directoryOffset = _reader.ReadInt32();

            _logger?.LogFormat("[{0}] Is IWAD? - {1}.", _fileName, _header_isIWAD.ToString());
            _logger?.LogFormat("[{0}] Lump count - {1}.", _fileName, _header_lumpCount.ToString());
            _logger?.LogFormat("[{0}] Directory offset - {1}.", _fileName, _header_directoryOffset.ToString());
        }

        private void ReadDirectory()
        {
            string logTemp = "";

            _fileStream.Seek(_header_directoryOffset, SeekOrigin.Begin);

            _lumps = new List<Lump>(_header_lumpCount);

            logTemp += String.Format("[{0}] Reading directory...\n", _fileName);

            for (int i = 0; i < _header_lumpCount; i++)
            {
                int offset = _reader.ReadInt32();
                int size = _reader.ReadInt32();
                string name = ReadString(8);

                _lumps.Add(new Lump(this, offset, size, name));

                logTemp += String.Format("[{0}] {1} | {2} | {3}.\n", _fileName, offset.ToString(), size.ToString(), name);
            }

            _logger?.Log(logTemp);
        }

        private void ReadTextures()
        {
            string logTemp = "";

            // PNAMES
            Lump pnames = (from p in _lumps where p.Name.StartsWith("PNAMES") select p).FirstOrDefault();

            if (pnames == null)
            {
                logTemp += String.Format("[{0}] No PNAMES lump.\n", _fileName);
                return;
            }

            if (pnames.Size == 0)
            {
                logTemp += String.Format("[{0}] PNAMES lump is empty.\n", _fileName);
                return;
            }

            _fileStream.Seek(pnames.Offset, SeekOrigin.Begin);

            uint patchCount = _reader.ReadUInt32();
            _patchNames = new List<string>((int)patchCount);

            for (int i = 0; i < patchCount; i++)
            {
                _patchNames.Add(ReadString(8));

                logTemp += String.Format("[{0}] {1}.\n", _fileName, _patchNames.Last());
            }



            // TEXTURE1
            Lump texture1 = (from p in _lumps where p.Name.StartsWith("TEXTURE1") select p).FirstOrDefault();

            if (texture1 == null)
            {
                logTemp += String.Format("[{0}] No TEXTURE1 lump.\n", _fileName);
                return;
            }

            if (texture1.Size == 0)
            {
                logTemp += String.Format("[{0}] TEXTURE1 lump is empty.\n", _fileName);
                return;
            }

            _fileStream.Seek(texture1.Offset, SeekOrigin.Begin);

            uint textureCount = _reader.ReadUInt32();
            if (_textureData != null) foreach (TextureData textureData in _textureData) textureData.Dispose();
            _textureData = new List<TextureData>((int)textureCount);

            _fileStream.Seek(textureCount * 4, SeekOrigin.Current);

            for (uint i = 0; i < textureCount; i++)
            {
                string name = ReadString(8);
                uint masked = _reader.ReadUInt32();
                ushort width = _reader.ReadUInt16();
                ushort height = _reader.ReadUInt16();
                uint columnDirectory = _reader.ReadUInt32();
                ushort numPatches = _reader.ReadUInt16();

                _textureData.Add(new TextureData(texture1, name, masked, width, height, columnDirectory, numPatches));

                logTemp += String.Format("[{0}] {1} | {2} | {3}.\n", _fileName, name, width.ToString(), height.ToString());

                for (int k = 0; k < numPatches; k++)
                {
                    short originX = _reader.ReadInt16();
                    short originY = _reader.ReadInt16();
                    ushort patchid = _reader.ReadUInt16();
                    ushort stepDir = _reader.ReadUInt16();
                    ushort colormap = _reader.ReadUInt16();

                    _textureData.LastOrDefault()?.Patches.Add(new PatchData(texture1, _patchNames[patchid], originX, originY, patchid, stepDir, colormap));

                    logTemp += String.Format("[{0}] [{1}] {2} | {3} | {4}.\n", _fileName, name, _patchNames[patchid], originX.ToString(), originY.ToString());
                }
            }



            // TEXTURE2
            Lump texture2 = (from p in _lumps where p.Name.StartsWith("TEXTURE2") select p).FirstOrDefault();

            if (texture2 == null)
            {
                logTemp += String.Format("[{0}] No TEXTURE2 lump.\n", _fileName);
                return;
            }

            if (texture2.Size == 0)
            {
                logTemp += String.Format("[{0}] TEXTURE2 lump is empty.\n", _fileName);
                return;
            }

            _fileStream.Seek(texture2.Offset, SeekOrigin.Begin);

            textureCount = _reader.ReadUInt32();

            _fileStream.Seek(textureCount * 4, SeekOrigin.Current);

            for (uint i = 0; i < textureCount; i++)
            {
                string name = ReadString(8);
                uint masked = _reader.ReadUInt32();
                ushort width = _reader.ReadUInt16();
                ushort height = _reader.ReadUInt16();
                uint columnDirectory = _reader.ReadUInt32();
                ushort numPatches = _reader.ReadUInt16();

                _textureData.Add(new TextureData(texture2, name, masked, width, height, columnDirectory, numPatches));

                logTemp += String.Format("[{0}] {1} | {2} | {3}.\n", _fileName, name, width.ToString(), height.ToString());

                for (int k = 0; k < numPatches; k++)
                {
                    short originX = _reader.ReadInt16();
                    short originY = _reader.ReadInt16();
                    ushort patchid = _reader.ReadUInt16();
                    ushort stepDir = _reader.ReadUInt16();
                    ushort colormap = _reader.ReadUInt16();

                    _textureData.LastOrDefault()?.Patches.Add(new PatchData(texture2, _patchNames[patchid], originX, originY, patchid, stepDir, colormap));

                    logTemp += String.Format("[{0}] [{1}] {2} | {3} | {4}.\n", _fileName, name, _patchNames[patchid], originX.ToString(), originY.ToString());
                }
            }

            _logger?.Log(logTemp);
        }

        public bool PNAMEStoCSV(string csvPath)
        {
            if (_patchNames == null)
            {
                _logger?.Log($"[{_fileName}] No PNAMES lump.");
                return false;
            }

            _logger?.Log($"[{_fileName}] Starting export PNAMES to csv...");

            StringBuilder csvData = new StringBuilder();

            csvData.AppendLine("Name");

            foreach (string str in _patchNames)
                csvData.AppendLine(str);

            File.WriteAllText(csvPath, csvData.ToString());

            _logger?.Log($"[{_fileName}] Export PNAMES to csv - SUCCESS!");

            return true;
        }

        public bool TEXTUREStoCSV(string csvPath) 
        {
            if (_textureData == null)
            {
                _logger?.Log($"[{_fileName}] No TEXTUREx lump.");
                return false;
            }

            _logger?.Log($"[{_fileName}] Starting export TEXTUREs to csv...");

            StringBuilder csvData = new StringBuilder();

            csvData.AppendLine("Name,Width,Height");

            foreach (TextureData textureData in _textureData)
                csvData.AppendLine($"{textureData.Name},{textureData.Width},{textureData.Height}");

            File.WriteAllText(csvPath, csvData.ToString());

            _logger?.Log($"[{_fileName}] Export TEXTUREs to csv - SUCCESS!");

            return true; 
        }

        public bool TEXTUREwithPATCHEStoCSV(string csvPath)
        {
            if (_patchNames == null || _textureData == null)
            {
                _logger?.Log($"[{_fileName}] No PNAMES or TEXTUREx lump.");
                return false;
            }

            _logger?.Log($"[{_fileName}] Starting export TEXTUREs with PATCHES to csv...");

            StringBuilder csvData = new StringBuilder();

            csvData.AppendLine("Texture Name,Texture Width,Texture Height,Patch Name,Patch Origin X,Patch Origin Y");

            foreach (TextureData textureData in _textureData)
            foreach (PatchData patchData in textureData.Patches)
                csvData.AppendLine($"{textureData.Name},{textureData.Width},{textureData.Height},{patchData.Name},{patchData.OriginX},{patchData.OriginY}");

            File.WriteAllText(csvPath, csvData.ToString());

            _logger?.Log($"[{_fileName}] Export TEXTUREs with PATCHES to csv - SUCCESS!");

            return true;
        }

        public bool FLATStoCSV(string csvPath)
        {
            int startIndex = _lumps.FindIndex(x => x.Name == "FF_START" || x.Name == "F_START");
            int endIndex = _lumps.FindIndex(x => x.Name == "FF_END" || x.Name == "F_END");

            if (startIndex == -1 && endIndex == -1)
            {
                _logger?.Log($"[{_fileName}] No FF_START/F_START and FF_END/F_END lump.");
                return false;
            }
            else if (startIndex == -1)
            {
                _logger?.Log($"[{_fileName}] No FF_START or F_START lump.");
                return false;
            }
            else if (endIndex == -1)
            {
                _logger?.Log($"[{_fileName}] No FF_END or F_END lump.");
                return false;
            }

            _logger?.Log($"[{_fileName}] Starting export flat textures data to csv...");

            StringBuilder csvData = new StringBuilder();

            csvData.AppendLine("Flat texture name");

            for (int i = startIndex + 1; i != endIndex && i < _lumps.Count; i++)
                csvData.AppendLine(_lumps[i].Name);

            File.WriteAllText(csvPath, csvData.ToString());

            _logger?.Log($"[{_fileName}] Export flat textures data to csv - SUCCESS!");

            return true;
        }

        public bool TXtoCSV(string csvPath)
        {
            int startIndex = _lumps.FindIndex(x => x.Name == "TX_START");
            int endIndex = _lumps.FindIndex(x => x.Name == "TX_END");

            if (startIndex == -1 && endIndex == -1)
            {
                _logger?.Log($"[{_fileName}] No TX_START lump.");
                return false;
            }
            else if (startIndex == -1)
            {
                _logger?.Log($"[{_fileName}] No TX_START lump.");
                return false;
            }
            else if (endIndex == -1)
            {
                _logger?.Log($"[{_fileName}] No TX_END lump.");
                return false;
            }

            _logger?.Log($"[{_fileName}] Starting export TX_ textures data to csv...");

            StringBuilder csvData = new StringBuilder();

            csvData.AppendLine("TX_ texture name");

            for (int i = startIndex + 1; i != endIndex && i < _lumps.Count; i++)
                csvData.AppendLine(_lumps[i].Name);

            File.WriteAllText(csvPath, csvData.ToString());

            _logger?.Log($"[{_fileName}] Export TX_ textures data to csv - SUCCESS!");

            return true;
        }



        // Cleaning memory
        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;
            if (disposing)
            {
                if (_lumps != null) foreach (Lump lump in _lumps) lump.Dispose();
                if (_textureData != null) foreach (TextureData textureData in _textureData) textureData.Dispose();

                _reader?.Close();
                _fileStream?.Close();
            }

            isDisposed = true;
        }

        ~WAD()
        {
            Dispose(false);
        }
    }
}
