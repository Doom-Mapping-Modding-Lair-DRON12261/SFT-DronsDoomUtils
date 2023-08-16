using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DronDoomTexUtilsDLL
{
    public class TextureData : IDisposable
    {
        // Variables
        private bool isDisposed = false;

        private Lump _parenLump;
        private string _name;
        private uint _masked;
        private ushort _width;
        private ushort _height;
        private uint _columnDirectory;
        private ushort _numPatches;
        private List<PatchData> _patches;



        // Constructors
        public TextureData(Lump parentLump, string name, uint masked, ushort width, ushort height, uint columnDirectory, ushort numPathces)
        {
            _parenLump = parentLump;
            _name = name;
            _masked = masked;
            _width = width;
            _height = height;
            _columnDirectory = columnDirectory;
            _numPatches = numPathces;
            _patches = new List<PatchData>(_numPatches);
        }



        // Properties
        public Lump ParentLump => _parenLump;
        public string Name => _name;
        public uint Masked => _masked;
        public ushort Width => _width;
        public ushort Height => _height;
        public uint ColumnDirectory => _columnDirectory;
        public ushort NumPatches => _numPatches;
        public List<PatchData> Patches => _patches;



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
                _parenLump = null; 
                _name = null;
            }

            isDisposed = true;
        }

        ~TextureData()
        {
            Dispose(false);
        }
    }
}
