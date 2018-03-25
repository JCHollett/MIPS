namespace CS3725.MIPSMemory {

    using MIPSInstructions;
    using MIPSRegisters;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The MIPSLoader that loads all the relevant information from file selected  by a user
    /// </summary>
    public class MIPSLoader {

        /// <summary>
        /// This is the file reader
        /// </summary>
        private StreamReader reader;

        /// <summary>
        /// This is the DataMemory of the MIPS machine
        /// </summary>
        private DataMemory mem;

        /// <summary>
        /// This is the Registry of the MIPS machine
        /// </summary>
        private Registry reg;

        /// <summary>
        /// This is the Instruction ROM of the MIPS machine
        /// </summary>
        private IMemory imem;

        /// <summary>
        /// The constructor for a MIPSLoader. Finds the file that contains the instructions. Takes the three different components that it's going to load the data into.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="i"></param>
        /// <param name="r"></param>
        public MIPSLoader( DataMemory m , IMemory i , Registry r ) {
            mem = m;
            imem = i;
            reg = r;
            reader = null;
            foreach( var file in Directory.EnumerateFiles( Directory.GetCurrentDirectory() ).Where( x => x.Contains( ".txt" ) ).Select( x => x.Substring( x.LastIndexOf( '\\' ) + 1 ) ) ) {
                var str = string.Format( "Read MIPS file from: {0}? [Y/N] [ ]" , file );
                CUI.LastLength = str.Length;
                Console.Write( str );
                Console.SetCursorPosition( Console.CursorLeft - 2 , Console.CursorTop );
                bool stop = false;
                while( !stop ) {
                    switch( Console.ReadKey( true ).Key ) {
                        case ConsoleKey.Y:
                            stop = true;
                            this.reader = File.OpenText( file );
                            Console.Write( 'Y' );
                            Console.SetCursorPosition( 0 , Console.CursorTop );
                            Console.Write( string.Empty.PadLeft( CUI.LastLength ) );
                            Console.SetCursorPosition( 0 , Console.CursorTop );
                            return;

                        case ConsoleKey.N:
                            stop = true;
                            Console.Write( 'N' );
                            Console.SetCursorPosition( 0 , Console.CursorTop );
                            Console.Write( string.Empty.PadLeft( CUI.LastLength ) );
                            Console.SetCursorPosition( 0 , Console.CursorTop );
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Using the file supplied, loads all the values from file into the three relevant components given to the constructor
        /// </summary>
        /// <returns></returns>
        public uint Initialize() {
            if( reader == null ) {
                throw new FileNotFoundException( "File Not Found or No File" );
            }
            Instruction instr = 0;
            string binarystr = "";
            uint pc = 0;
            uint hex = 0x0;
            string current;
            uint lineN =0;
            while( !reader.EndOfStream ) {
                lineN++;
                if( ( current = reader.ReadLine().Trim() ).Length == 0 || current[ 0 ] == '#' ) continue;
                var line = Regex.Split( current, @"\s{1,}" );
                line[ 0 ] = line[ 0 ].ToLower();
                try {
                    if( Instruction.Operations.ContainsKey( line[ 0 ] ) ) {
                        if( line.Length != 4 && line.Length != 1 ) { throw new Exception( string.Format( "Incorrect Parameters: {0}" , current ) ); }
                        instr = 0;
                        binarystr = string.Empty;
                        byte rs,rt,rd;
                        short offset;
                        switch( Instruction.Operations[ line[ 0 ] ] ) {
                            //add
                            case 0:
                                if( !byte.TryParse( line[ 2 ] , out rs ) || !byte.TryParse( line[ 3 ] , out rt ) || !byte.TryParse( line[ 1 ] , out rd ) )
                                    throw new Exception( string.Format( "Bad operation: {0}" , current ) );
                                binarystr = Instruction.RTypeBinary( 0x00 , rs , rt , rd , 0x0 , 0x20 );
                                break;

                            //sub
                            case 1:
                                if( !byte.TryParse( line[ 2 ] , out rs ) || !byte.TryParse( line[ 3 ] , out rt ) || !byte.TryParse( line[ 1 ] , out rd ) )
                                    throw new Exception( string.Format( "Bad operation: {0}" , current ) );
                                binarystr = Instruction.RTypeBinary( 0x00 , byte.Parse( line[ 2 ] ) , byte.Parse( line[ 3 ] ) , byte.Parse( line[ 1 ] ) , 0x0 , 0x22 );

                                break;

                            //lw
                            case 2:
                                if( !byte.TryParse( line[ 3 ] , out rs ) || !byte.TryParse( line[ 1 ] , out rt ) || !short.TryParse( line[ 2 ] , out offset ) )
                                    throw new Exception( string.Format( "Bad operation: {0}" , current ) );
                                binarystr = Instruction.ITypeBinary( 0x23 , rs , rt , offset );
                                break;

                            //sw
                            case 3:
                                if( !byte.TryParse( line[ 3 ] , out rs ) || !byte.TryParse( line[ 1 ] , out rt ) || !short.TryParse( line[ 2 ] , out offset ) )
                                    throw new Exception( string.Format( "Bad operation: {0}" , current ) );
                                binarystr = Instruction.ITypeBinary( 0x2B , rs , rt , offset );
                                break;

                            //beq
                            case 4:
                                if( !byte.TryParse( line[ 1 ] , out rs ) || !byte.TryParse( line[ 2 ] , out rt ) || !short.TryParse( line[ 3 ] , out offset ) )
                                    throw new Exception( string.Format( "Bad operation: {0}" , current ) );
                                binarystr = Instruction.ITypeBinary( 0x04 , rs , rt , offset );

                                break;

                            case 5:
                                try {
                                    binarystr = Instruction.RTypeBinary( 0x00 , 0 , 0 , 0 , 0 , 0 );
                                } catch {
                                }
                                break;
                        }

                        instr = new Instruction( binarystr );
                        imem[ 4 * pc++ ] = instr;
                        continue;
                    }
                    if( line[ 0 ][ 0 ] == 'r' ) {
                        try {
                            byte addr;
                            int val;
                            if( !byte.TryParse( line[ 0 ].Substring( 1 ) , out addr ) ) {
                                throw new InvalidOperationException( string.Format( "Bad address: {0}" , line[ 0 ] ) );
                            };
                            if( !int.TryParse( line[ 1 ] , out val ) ) {
                                throw new InvalidOperationException( string.Format( "Bad value: {0}" , line[ 1 ] ) );
                            }
                            reg[ addr ] = BitConverter.GetBytes( val );
                        } catch( InvalidOperationException e ) {
                            throw new Exception( string.Format( "{1}" , lineN , e.Message ) );
                        } catch( IndexOutOfRangeException ) {
                            throw new Exception( string.Format( "Malformed Register statement: {1}" , current ) );
                        }
                        continue;
                    }
                    if( line.Length <= 1 ) {
                        throw new Exception( string.Format( "Malformed statement: {0}" , current ) );
                    } else if( line.Length == 2 ) {
                        if( uint.TryParse( line[ 0 ] , NumberStyles.Integer , CultureInfo.InvariantCulture , out hex ) || ( line[ 0 ].Length >= 3 && uint.TryParse( line[ 0 ].Substring( 2 ) , NumberStyles.HexNumber , CultureInfo.InvariantCulture , out hex ) ) ) {
                            try {
                                mem[ hex ] = BitConverter.GetBytes( Convert.ToInt32( line[ 1 ] ) );
                            } catch {
                                throw new Exception( string.Format( "Bad Memory value: {0}" , line[ 1 ] ) );
                            }
                            continue;
                        } else {
                            throw new Exception( string.Format( "Bad Memory address: {0}" , line[ 0 ] ) );
                        }
                    } else {
                        throw new Exception( string.Format( "Bad statement: {0}" , current ) );
                    }
                } catch( Exception e ) {
                    Console.WriteLine( string.Format( "Error on line {0};  {1}" , lineN , e.Message ) );
                    return 0;
                }
            }
            return ( pc << 2 ) + ( 5 << 2 );
        }
    }

    /// <summary>
    /// This is an abstract Memory type. It describes both Instruction ROM and Data Memory
    /// </summary>
    public abstract class Memory {

        /// <summary>
        /// This is a 2D array used to store data into memory format.
        /// </summary>
        private byte[][] cell;

        /// <summary>
        /// The size of this memory. Sizes are determined by 4 Bytes * Size. Meaning that a Size of 10 would only be able to store 10 Instructions or Data values.
        /// </summary>
        public uint Size { get; private set; }
        /// <summary>
        /// Returns the byte[] from memory representing a data value of some abstract kind. Sets and Gets byte[4] data values.
        /// EX: calling mem[0], mem[1], mem[2], or mem[3], will only return the byte[4] representing the first data value in memory
        /// Applies a modulus over the address to obtain the data values
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        public byte[ ] this[ uint Address ] {
            get {
                Address >>= 2;
                return cell[ Address ] != null ? cell[ Address ] : cell[ Address ] = new byte[ 4 ];
            }
            set {
                Address >>= 2;
                try {
                    Array.Copy( value , cell[ Address ] != null ? cell[ Address ] : cell[ Address ] = new byte[ 4 ] , 4 );
                } catch( Exception ) {
                    //
                }
            }
        }

        /// <summary>
        /// Constructor for an abstract memory type. Size of this memory is 4 Bytes * Size.
        /// </summary>
        /// <param name="size"></param>
        public Memory( uint size ) {
            this.cell = new byte[ this.Size = size ][ ];
        }
    }

    /// <summary>
    /// Instruction Memory has no special members, fields or methods. It can only save and read information.
    /// But, the program never saves data to the Instruction Memory after the program starts. Although, it would be possible.
    /// Found in the Fetch Stage
    /// </summary>
    public class IMemory : Memory {

        /// <summary>
        /// Instruction Memory constructor. Same as the Base constructor.
        /// </summary>
        /// <param name="size"></param>
        public IMemory( uint size ) : base( size ) {
        }
    }

    /// <summary>
    /// Data Memory has special members and fields.
    /// Can save and load data more permanently
    /// Found in the Memory stage.
    /// </summary>
    public class DataMemory : Memory {

        /// <summary>
        /// The address to read represented by a UInt32
        /// </summary>
        public uint ReadAddress;

        /// <summary>
        /// The address to write represented by a UInt32
        /// </summary>
        public uint WriteAddress;

        /// <summary>
        /// The byte[4] value to Write to the WriteAddress location in memory
        /// </summary>
        public byte[] WriteData;

        /// <summary>
        /// The byte[4] value to Read from the ReadAddress location in memory
        /// </summary>
        public byte[] ReadData;

        /// <summary>
        /// Sending a True to this Property will cause Memory to perform a MemWrite operation.
        /// Any information stored by this DataMemory component is written to memory following the signal.
        /// </summary>
        public bool MemWrite {
            set {
                if( value )
                    this[ WriteAddress ] = WriteData;
            }
        }
        /// <summary>
        /// Sending a True to this Property will cause Memory to perform a MemRead operation.
        /// Any information stored by this DataMemory component is read from memory following the signal.
        /// </summary>
        public bool MemRead {
            set {
                if( value )
                    ReadData = this[ ReadAddress ];
            }
        }

        /// <summary>
        /// The constructor for a Data Memory component. Size of this memory is 4 Bytes * Size.
        /// Default byte[4] is assigned to WriteData and ReadData to prevent null errors.
        /// By default ReadAddress/WriteAddress is zero.
        /// </summary>
        /// <param name="size"></param>
        public DataMemory( uint size ) : base( size ) {
            WriteData = new byte[ 4 ];
            ReadData = new byte[ 4 ];
            ReadAddress = 0;
            WriteAddress = 0;
        }
    }
}