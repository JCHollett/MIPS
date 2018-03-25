namespace CS3725.MIPSRegisters {

    using MIPSInstructions;
    using System;

    public struct Register {
        private byte[] d;
        public byte[ ] Data {
            get { return this.d; }
            set { this.d = value; }
        }

        public Register( byte[ ] b ) {
            this.d = new byte[ 4 ];
            Array.Copy( b , d , 4 );
        }

        public static implicit operator Register( byte[ ] b ) {
            return new Register( b );
        }

        public override String ToString() {
            return BitConverter.ToInt32( d , 0 ).ToString();
        }
    }

    public class Registry {
        private Instruction i;
        public Instruction Input {
            get { return i; }
            set { i = value; }
        }
        public byte ReadRegister1 {
            get {
                return i.RS;
            }
        }
        public byte ReadRegister2 {
            get {
                return i.RT;
            }
        }
        public byte[ ] ReadData1 {
            get {
                return this[ this.ReadRegister1 ].Data;
            }
        }
        public byte[ ] ReadData2 {
            get {
                return this[ this.ReadRegister2 ].Data;
            }
        }
        public byte WriteRegister;
        public byte[] WriteData;
        public bool RegWrite {
            set {
                if( value ) {
                    this[ WriteRegister ] = WriteData;
                }
            }
        }
        private Register[] registers;
        public Register this[ byte r ] {
            get {
                if( r >= 0 && r < registers.Length )
                    return registers[ r ];
                else
                    throw new InvalidOperationException( string.Format( "Bad Registry Address: ${0}" , r ) );
            }
            set {
                if( r > 0 && r < registers.Length )
                    registers[ r ].Data = value.Data;
                else
                    throw new InvalidOperationException( string.Format( "Bad Registry Address: ${0}" , r ) );
            }
        }

        public Registry() {
            registers = new Register[ 32 ];
            registers[ 0 ] = new byte[ 4 ];
        }
    }
}