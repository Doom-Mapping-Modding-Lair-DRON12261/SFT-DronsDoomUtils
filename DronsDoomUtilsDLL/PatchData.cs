using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DronDoomTexUtilsDLL
{
    public class PatchData : IDisposable
    {
        // Variables
        private bool isDisposed = false;

        private Lump _parentLump = null;
        private string _name = "";
        private short _originX = 0;
        private short _originY = 0;
        private ushort _patchid = 0;
        private ushort _stepDir = 0;
        private ushort _colormap = 0;



        // Constructors
        public PatchData(Lump parentLump, string name, short originX, short originY, ushort patchid, ushort stepDir, ushort colormap)
        {
            _parentLump = parentLump;
            _name = name;
            _originX = originX;
            _originY = originY;
            _patchid = patchid;
            _stepDir = stepDir;
            _colormap = colormap;
        }



        // Properties
        public Lump ParentLump => _parentLump;
        public string Name => _name;
        public short OriginX => _originX;
        public short OriginY => _originY;
        public ushort Patchid => _patchid;
        public ushort StepDir => _stepDir;
        public ushort Colormap => _colormap;



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
                
            }

            isDisposed = true;
        }

        ~PatchData()
        {
            Dispose(false);
        }
    }
}
