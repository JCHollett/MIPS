namespace CS3725.MIPSInstructions {

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The instruction register that contains the instruction in the fetch stage
    /// </summary>
    public class IRegister {
        /// <summary>
        /// This output returns a new instruction based off of the UInt32 data in this register
        /// </summary>
        public Instruction Output { get { return new Instruction( this.data ); } }
        /// <summary>
        /// Input is bytes[4] from the instruction ROM and converts it into the UInt32 Data
        /// </summary>
        public byte[ ] Input {
            set {
                this.data = BitConverter.ToUInt32( value , 0 );
            }
        }

        /// <summary>
        /// Current data held by this register
        /// </summary>
        private uint data;

        /// <summary>
        /// Create a register with NOP valued data
        /// </summary>
        public IRegister() {
            this.data = 0x0;
        }

        /// <summary>
        /// Create a register with NON-NOP valued data
        /// </summary>
        /// <param name="bytes"></param>
        private IRegister( byte[ ] bytes ) : this() {
            this.Input = bytes;
        }

        /// <summary>
        /// Implicit cast for a register with NON-NOP valued data
        /// </summary>
        /// <param name="bytes"></param>
        public static implicit operator IRegister( byte[ ] bytes ) {
            return new IRegister( bytes );
        }
    }

    /// <summary>
    /// This is an Instruction created in the fetch stage
    /// </summary>
    public struct Instruction {

        /// <summary>
        /// these are the supported instruction operations
        /// </summary>
        public static Dictionary<string, int> Operations = new Dictionary<string, int>{
            { "add",0 },
            { "sub",1 },
            { "lw",2 },
            { "sw", 3},
            { "beq",4},
            { "nop", 5 }
        };

        /// <summary>
        /// This is the simplified string version of the operation
        /// </summary>
        private string Semantic;

        /// <summary>
        /// This is a static readonly constant for offsetting bit reading
        /// </summary>
        private static readonly byte Zero = 48;

        /// <summary>
        /// The UInt32 value of this instruction
        /// </summary>
        private uint value;

        /// <summary>
        /// The OpCode contained by this instruction
        /// </summary>
        public byte OpCode {
            get { return ( byte )( value >> 26 ); }
        }
        /// <summary>
        /// The RS value contained by this instruction
        /// </summary>
        public byte RS {
            get { return ( byte )( ( value << 6 ) >> 27 ); }
        }
        /// <summary>
        /// The RT value contained by this instruction
        /// </summary>
        public byte RT {
            get { return ( byte )( ( value << 11 ) >> 27 ); }
        }
        /// <summary>
        /// The RD value contained by this instruction
        /// </summary>
        public byte RD {
            get { return ( byte )( ( value << 16 ) >> 27 ); }
        }
        /// <summary>
        /// The SHAMT value contained by this instruction
        /// </summary>
        public byte SHAMT {
            get { return ( byte )( ( value << 21 ) >> 27 ); }
        }
        /// <summary>
        /// The FUNCT value contained by this instruction
        /// </summary>
        public byte FUNCT {
            get { return ( byte )( value & 0x0000003F ); }
        }
        /// <summary>
        /// The OFFSET value contained by this instruction
        /// </summary>
        public short OFFSET {
            get { return ( short )( value & 0x0000FFFF ); }
        }
        /// <summary>
        /// The TARGET value contained by this instruction UNUSED
        /// </summary>
        public uint TARGET {
            get { return value & 0x03FFFFFF; }
        }
        /// <summary>
        /// The UInt32 Property that points to the variable that represents this instruction
        /// </summary>
        public uint Data { get { return this.value; } }

        /// <summary>
        /// Constructor for an Instruction using a UInt32 value
        /// </summary>
        /// <param name="val"></param>
        public Instruction( uint val ) {
            this.value = val;
            this.Semantic = "";
            this.Semantic = SemanticOp( ref this );
        }

        /// <summary>
        /// Constructor for an Instruction using a Binary string. Can be spaced binary if it suits the user
        /// </summary>
        /// <param name="binary"></param>
        public Instruction( string binary ) {
            this.Semantic = "";
            var bin = binary.Replace(" ", "");
            if( bin.Length != 32 ) throw new FormatException( string.Format( "Bad Instruction Formart: {0}" , bin ) );
            this.value = 0;
            int temp = 0;
            int offset = 0;

            foreach( var bit in bin.Reverse().Select( Bit => ( Bit - Zero ) ) ) {
                if( !( bit == 0 || bit == 1 ) ) throw new FormatException( "Bad Binary Digit" );
                temp |= ( bit << offset++ );
            }
            this.value = ( uint )temp;
            this.Semantic = SemanticOp( ref this );
        }

        /// <summary>
        /// Creates the SemanticOp string that is output in the CUI class
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static string SemanticOp( ref Instruction r ) {
            string x = string.Empty;
            switch( r.OpCode ) {
                case 0:
                    switch( r.FUNCT ) {
                        case 32:
                            x = string.Format( "add ${0},${1},${2}" , r.RD , r.RS , r.RT );
                            break;

                        case 34:
                            x = string.Format( "sub ${0},${1},${2}" , r.RD , r.RS , r.RT );
                            break;
                    }
                    break;

                case 35:
                    x = string.Format( "lw ${0}, {1}(${2})" , r.RT , r.OFFSET , r.RS );
                    break;

                case 43:
                    x = string.Format( "sw ${0}, {1}(${2})" , r.RT , r.OFFSET , r.RS );
                    break;

                case 4:
                    x = string.Format( "beq ${0}, ${1}, {2}" , r.RS , r.RT , r.OFFSET );
                    break;
            }
            if( r.value == 0 ) {
                x = "nop";
            }
            return x;
        }

        /// <summary>
        /// Constructor for an R-Type instruction off of individual values
        /// </summary>
        /// <param name="op"></param>
        /// <param name="rs"></param>
        /// <param name="rt"></param>
        /// <param name="rd"></param>
        /// <param name="shamt"></param>
        /// <param name="funct"></param>
        public Instruction( byte op , byte rs , byte rt , byte rd , byte shamt , byte funct ) : this( RTypeBinary( op , rs , rt , rd , shamt , funct ) ) {
        }

        /// <summary>
        /// Constructor for an I-Type instruction off of individual values
        /// </summary>
        /// <param name="op"></param>
        /// <param name="rs"></param>
        /// <param name="rt"></param>
        /// <param name="offset"></param>
        public Instruction( byte op , byte rs , byte rt , short offset ) : this( ITypeBinary( op , rs , rt , offset ) ) {
        }

        /// <summary>
        /// Create an RTypeBinary string using individual values
        /// </summary>
        /// <param name="op"></param>
        /// <param name="rs"></param>
        /// <param name="rt"></param>
        /// <param name="rd"></param>
        /// <param name="shamt"></param>
        /// <param name="funct"></param>
        /// <returns></returns>
        public static string RTypeBinary( byte op , byte rs , byte rt , byte rd , byte shamt , byte funct ) {
            return Convert.ToString( op & 0x3F , 2 ).PadLeft( 6 , '0' ) +
            Convert.ToString( rs & 0x1F , 2 ).PadLeft( 5 , '0' ) +
            Convert.ToString( rt & 0x1F , 2 ).PadLeft( 5 , '0' ) +
            Convert.ToString( rd & 0x1F , 2 ).PadLeft( 5 , '0' ) +
            Convert.ToString( shamt & 0x1F , 2 ).PadLeft( 5 , '0' ) +
            Convert.ToString( funct & 0x3F , 2 ).PadLeft( 6 , '0' );
        }

        /// <summary>
        /// Create an ITypeBinary string using individual values
        /// </summary>
        /// <param name="op"></param>
        /// <param name="rs"></param>
        /// <param name="rt"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string ITypeBinary( byte op , byte rs , byte rt , short offset ) {
            return Convert.ToString( op & 0x3F , 2 ).PadLeft( 6 , '0' ) +
                Convert.ToString( rs & 0x3F , 2 ).PadLeft( 5 , '0' ) +
                Convert.ToString( rt & 0x3F , 2 ).PadLeft( 5 , '0' ) +
                Convert.ToString( offset , 2 ).PadLeft( 16 , '0' );
        }

        /// <summary>
        /// Implicit cast for UInt32 to Instruction
        /// </summary>
        /// <param name="v"></param>
        public static implicit operator Instruction( uint v ) {
            return new Instruction( v );
        }

        /// <summary>
        /// Implicit cast for a Binary String to Instruction
        /// </summary>
        /// <param name="bits"></param>
        public static implicit operator Instruction( string bits ) {
            return new Instruction( bits );
        }

        /// <summary>
        /// Explicit cast for an Instruction to UInt32
        /// </summary>
        /// <param name="r"></param>
        public static explicit operator uint( Instruction r ) {
            return r.value;
        }

        /// <summary>
        /// Implicit cast for an instruction to byte[4] array
        /// </summary>
        /// <param name="r"></param>
        public static implicit operator byte[ ] ( Instruction r ) {
            return BitConverter.GetBytes( r.value );
        }

        /// <summary>
        /// The Semantic string representation is returned by this
        /// </summary>
        /// <returns></returns>
        public override String ToString() {
            return string.Format( "{0}" , this.Semantic );
        }
    }
}