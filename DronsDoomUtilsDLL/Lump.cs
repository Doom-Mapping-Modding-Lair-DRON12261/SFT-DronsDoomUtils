using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace DronDoomTexUtilsDLL
{
    public class Lump : IDisposable
    {
        // Variables
        private bool isDisposed = false;

        private WAD _parentWAD = null;
        private int _offset = 0;
        private int _size = 0;
        private string _name = "";



        // Constructors
        public Lump(WAD parentWAD) 
        {
            _parentWAD = parentWAD;
        }

        public Lump(WAD parentWAD, int offset, int size, string name)
        {
            _parentWAD = parentWAD;
            _offset = offset;
            _size = size;
            _name = name;
        }



        // Properties
        public int Offset => _offset;
        public int Size => _size;
        public string Name => _name;



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

        ~Lump()
        {
            Dispose(false);
        }
    }
}
