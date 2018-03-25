using System;
using CS3725.MIPSBuffer;
using CS3725.MIPSMemory;
using CS3725.MIPSRegisters;
using System.Linq;
using CS3725.MIPSInstructions;
using CS3725.MIPSControl;

namespace CS3725 {

    /// <summary>
    /// Renders the inputs and outputs of the mips architecture
    /// </summary>
    public class CUI {

        /// <summary>
        /// Reference to the Instruction ROM
        /// </summary>
        private IMemory IMEM;

        /// <summary>
        /// A Reference to data memory of the mips
        /// </summary>
        private DataMemory MEM;

        /// <summary>
        /// A reference to the registry
        /// </summary>
        private Registry REG;

        /// <summary>
        /// Used to clear the console file input prompting
        /// </summary>
        public static int LastLength;

        /// <summary>
        /// The size of an integer in characters
        /// </summary>
        public static int IntWidth = Int32.MinValue.ToString().Length;

        /// <summary>
        /// The size of a unsigned integer in characters
        /// </summary>
        public static int UIntWidth = UInt32.MaxValue.ToString().Length;

        /// <summary>
        /// The difference stages with spaces to make their character width even for fancier output
        /// </summary>
        private static string[] stage = {" Fetch", "Decode", " Execute", "Memory", " WriteBack" };

        /// <summary>
        /// Constructor for a CUI
        /// </summary>
        /// <param name="imem"></param>
        /// <param name="mem"></param>
        /// <param name="reg"></param>
        internal CUI( ref IMemory imem , ref DataMemory mem , ref Registry reg ) {
            this.IMEM = imem;
            this.MEM = mem;
            this.REG = reg;
        }

        /// <summary>
        /// Formatting a the data bytes[] into an 32-bit integer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        internal static string FormatByteArray( byte[ ] data ) {
            return string.Format( "{0}" , BitConverter.ToInt32( data , 0 ).ToString().PadLeft( IntWidth , ' ' ) );
        }

        /// <summary>
        /// Display the program counter with the value passed to this
        /// </summary>
        /// <param name="PC"></param>
        internal void PCDisplay( uint PC ) {
            string banner = "PC [byte]   ";
            int offsetX = 0, offsetY = 0;
            Console.SetCursorPosition( offsetX , offsetY + 2 );
            Console.Write( string.Empty.PadLeft( UIntWidth + 10 , '=' ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( "||{0}||" , banner.PadLeft( UIntWidth + 6 , ' ' ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Empty.PadLeft( UIntWidth + 10 , '=' ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( "||{0}||" , PC.ToString().PadLeft( UIntWidth + 6 , ' ' ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Empty.PadLeft( UIntWidth + 10 , '=' ) );
            Console.CursorTop = offsetY;
        }

        /// <summary>
        /// Displays the Fetch information
        /// </summary>
        /// <param name="FetchBufferL"></param>
        /// <param name="BRANCH"></param>
        /// <param name="PC4"></param>
        /// <param name="PCBranch"></param>
        internal void FetchDisplay( ref FetchDecodeBuffer FetchBufferL , bool BRANCH , uint PC4 , uint PCBranch ) {
            int labelWidth = 12; int dataWidth = 16;

            int offsetX = 21, offsetY = 0;
            Console.SetCursorPosition( offsetX , offsetY + 2 );
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||" , string.Empty.PadLeft( labelWidth , ' ' ) , stage[ 0 ].PadLeft( dataWidth - ( ( dataWidth - stage[ 0 ].Length ) / 2 ) , ' ' ).PadRight( dataWidth , ' ' ) ) );
            //Console.CursorTop += 1;

            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||" , "Semantics".PadRight( labelWidth , ' ' ) , FetchBufferL.Instr.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "BRANCH".PadRight( labelWidth , ' ' ) , BRANCH.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "PC4".PadRight( labelWidth , ' ' ) , PC4.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "PCBranch".PadRight( labelWidth , ' ' ) , PCBranch.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.SetCursorPosition( 0 , 0 );
        }

        /// <summary>
        /// Displays the Decode information
        /// </summary>
        /// <param name="FetchBufferR"></param>
        /// <param name="DecodeBufferL"></param>
        /// <param name="RegWrite"></param>
        internal void DecodeDisplay( ref FetchDecodeBuffer FetchBufferR , ref DecodeExecuteBuffer DecodeBufferL , bool RegWrite ) {
            int labelWidth = 12; int dataWidth = 16;

            int offsetX = 21, offsetY = 8;
            Console.SetCursorPosition( offsetX , offsetY + 2 );
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||" , string.Empty.PadLeft( labelWidth , ' ' ) , stage[ 1 ].PadLeft( dataWidth - ( ( dataWidth - stage[ 1 ].Length ) / 2 ) , ' ' ).PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||" , "Semantics".PadRight( labelWidth , ' ' ) , FetchBufferR.Instr.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "OpCode".PadRight( labelWidth , ' ' ) , FetchBufferR.Instr.OpCode.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "RS".PadRight( labelWidth , ' ' ) , FetchBufferR.Instr.RS.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "RT".PadRight( labelWidth , ' ' ) , FetchBufferR.Instr.RT.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "RD".PadRight( labelWidth , ' ' ) , FetchBufferR.Instr.RD.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "FUNCT".PadRight( labelWidth , ' ' ) , FetchBufferR.Instr.FUNCT.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "OFFSET".PadRight( labelWidth , ' ' ) , FetchBufferR.Instr.OFFSET.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "ReadData1".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( REG.ReadData1 , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "ReadData2".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( REG.ReadData2 , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "WriteRgster".PadRight( labelWidth , ' ' ) , REG.WriteRegister.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "WriteData".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( REG.WriteData , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "RegWrite".PadRight( labelWidth , ' ' ) , RegWrite.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.SetCursorPosition( 0 , 0 );
        }

        /// <summary>
        /// Displays the Execute information
        /// </summary>
        /// <param name="DecodeBufferR"></param>
        /// <param name="ExecuteBufferL"></param>
        internal void ExecuteDisplay( ref DecodeExecuteBuffer DecodeBufferR , ref ExecuteMEMBuffer ExecuteBufferL ) {
            int labelWidth = 12; int dataWidth = 16;

            int offsetX = 21, offsetY = 24;
            Console.SetCursorPosition( offsetX , offsetY + 2 );
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||" , string.Empty.PadLeft( labelWidth , ' ' ) , stage[ 2 ].PadLeft( dataWidth - ( ( dataWidth - stage[ 2 ].Length ) / 2 ) , ' ' ).PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||" , "Semantics".PadRight( labelWidth , ' ' ) , DecodeBufferR.Instr.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "ALUOp0".PadRight( labelWidth , ' ' ) , DecodeBufferR.vector.ALUOp0.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "ALUOp1".PadRight( labelWidth , ' ' ) , DecodeBufferR.vector.ALUOp1.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "ALUresult".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( ExecuteBufferL.ALUResult , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "ALUzero".PadRight( labelWidth , ' ' ) , ExecuteBufferL.Zero.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "ALUSrc".PadRight( labelWidth , ' ' ) , DecodeBufferR.vector.ALUSrc.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "OFFSET".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( DecodeBufferR.OFFSET , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "ReadData1".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( DecodeBufferR.ReadData1 , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "ReadData2".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( DecodeBufferR.ReadData2 , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "PC4".PadRight( labelWidth , ' ' ) , DecodeBufferR.PC4.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "PCBranch".PadRight( labelWidth , ' ' ) , ExecuteBufferL.PCBranch.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "RegDst".PadRight( labelWidth , ' ' ) , DecodeBufferR.vector.RegDst.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "RT".PadRight( labelWidth , ' ' ) , DecodeBufferR.RT.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "RD".PadRight( labelWidth , ' ' ) , DecodeBufferR.RD.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.SetCursorPosition( 0 , 0 );
        }

        /// <summary>
        /// Displays the Memory information
        /// </summary>
        /// <param name="ExecuteBufferR"></param>
        /// <param name="MEMBufferL"></param>
        /// <param name="isBranch"></param>
        internal void MEMDisplay( ref ExecuteMEMBuffer ExecuteBufferR , ref MEMWBBuffer MEMBufferL , bool isBranch ) {
            int labelWidth = 12; int dataWidth = 16;
            int offsetX = 56, offsetY = 0;
            Console.SetCursorPosition( offsetX , offsetY + 2 );
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||" , string.Empty.PadLeft( labelWidth , ' ' ) , stage[ 3 ].PadLeft( dataWidth - ( ( dataWidth - stage[ 3 ].Length ) / 2 ) , ' ' ).PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||" , "Semantics".PadRight( labelWidth , ' ' ) , ExecuteBufferR.Instr.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "MemRead".PadRight( labelWidth , ' ' ) , ExecuteBufferR.vector.MemRead.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "MemWrite".PadRight( labelWidth , ' ' ) , ExecuteBufferR.vector.MemWrite.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "Branch".PadRight( labelWidth , ' ' ) , ExecuteBufferR.vector.Branch.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "Zero".PadRight( labelWidth , ' ' ) , ExecuteBufferR.Zero.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "BranchSgnl".PadRight( labelWidth , ' ' ) , isBranch.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "PCBranch".PadRight( labelWidth , ' ' ) , ExecuteBufferR.PCBranch.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "RegDstAddr".PadRight( labelWidth , ' ' ) , ExecuteBufferR.DEST.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "ALUresult".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( ExecuteBufferR.ALUResult , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "ReadAddress".PadRight( labelWidth , ' ' ) , MEM.ReadAddress.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "WriteAddress".PadRight( labelWidth , ' ' ) , MEM.WriteAddress.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "WriteData".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( MEM.WriteData , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "ReadData".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( MEM.ReadData , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.SetCursorPosition( 0 , 0 );
        }

        /// <summary>
        /// Displays the WriteBack information
        /// </summary>
        /// <param name="MEMBufferR"></param>
        internal void WBDisplay( ref MEMWBBuffer MEMBufferR ) {
            int labelWidth = 12; int dataWidth = 16;
            int offsetX = 56, offsetY = 17 ;
            Console.SetCursorPosition( offsetX , offsetY + 2 );
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||" , string.Empty.PadLeft( labelWidth , ' ' ) , stage[ 4 ].PadLeft( dataWidth - ( ( dataWidth - stage[ 4 ].Length ) / 2 ) , ' ' ).PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||" , "Semantics".PadRight( labelWidth , ' ' ) , MEMBufferR.Instr.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "RegWrite".PadRight( labelWidth , ' ' ) , MEMBufferR.vector.RegWrite.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "MemToReg".PadRight( labelWidth , ' ' ) , MEMBufferR.vector.MemToReg.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "ReadData".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( MEMBufferR.ReadData , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "ALUresult".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( MEMBufferR.ALUResult , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||" , "RegDstAddr".PadRight( labelWidth , ' ' ) , MEMBufferR.DEST.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.SetCursorPosition( 0 , 0 );
        }

        /// <summary>
        /// Displays the IF/ID information
        /// </summary>
        /// <param name="buff"></param>
        internal void FetchDecodeBufferDisplay( params FetchDecodeBuffer[ ] buff ) {
            int offsetX = 91, offsetY = 2, labelWidth = 12, dataWidth=16;
            string banner = "IF/ID";
            Console.SetCursorPosition( offsetX , offsetY );
            Console.Write( string.Format( "=={0}=={1}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , string.Empty.PadLeft( labelWidth , ' ' ) , ( banner + "[0]" ).PadLeft( dataWidth - ( ( dataWidth - banner.Length - 3 ) / 2 ) , ' ' ).PadRight( dataWidth , ' ' ) , ( banner + "[1]" ).PadLeft( dataWidth - ( ( dataWidth - banner.Length - 3 ) / 2 ) , ' ' ).PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "Instruction".PadRight( labelWidth , ' ' ) , buff[ 0 ].Instr.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].Instr.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "PC4".PadRight( labelWidth , ' ' ) , buff[ 0 ].PC4.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].PC4.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
        }

        /// <summary>
        /// Displays the ID/EX Information
        /// </summary>
        /// <param name="buff"></param>
        internal void DecodeExecuteBufferDisplay( params DecodeExecuteBuffer[ ] buff ) {
            int offsetX = 91, offsetY = 8, labelWidth = 12, dataWidth=16;
            string banner = "ID/EX";
            Console.SetCursorPosition( offsetX , offsetY );
            Console.Write( string.Format( "=={0}=={1}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , string.Empty.PadLeft( labelWidth , ' ' ) , ( banner + "[0]" ).PadLeft( dataWidth - ( ( dataWidth - banner.Length - 3 ) / 2 ) , ' ' ).PadRight( dataWidth , ' ' ) , ( banner + "[1]" ).PadLeft( dataWidth - ( ( dataWidth - banner.Length - 3 ) / 2 ) , ' ' ).PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "RegDst".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.RegDst.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.RegDst.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "ALUop0".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.ALUOp0.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.ALUOp0.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "ALUop1".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.ALUOp1.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.ALUOp1.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "ALUSrc".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.ALUSrc.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.ALUSrc.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "Branch".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.Branch.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.Branch.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "MemRead".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.MemRead.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.MemRead.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "MemWrite".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.MemWrite.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.MemWrite.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "RegWrite".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.RegWrite.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.RegWrite.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "MemToReg".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.MemToReg.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.MemToReg.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "PC4".PadRight( labelWidth , ' ' ) , buff[ 0 ].PC4.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].PC4.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "ReadData1".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( buff[ 0 ].ReadData1 , 0 ).ToString().PadRight( dataWidth , ' ' ) , BitConverter.ToInt32( buff[ 1 ].ReadData1 , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "ReadData2".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( buff[ 0 ].ReadData2 , 0 ).ToString().PadRight( dataWidth , ' ' ) , BitConverter.ToInt32( buff[ 1 ].ReadData2 , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "OFFSET".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( buff[ 0 ].OFFSET , 0 ).ToString().PadRight( dataWidth , ' ' ) , BitConverter.ToInt32( buff[ 1 ].OFFSET , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "RT".PadRight( labelWidth , ' ' ) , buff[ 0 ].RT.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].RT.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "RD".PadRight( labelWidth , ' ' ) , buff[ 0 ].RD.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].RD.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
        }

        /// <summary>
        /// Displays the EX/MEM information
        /// </summary>
        /// <param name="buff"></param>
        internal void ExecuteMEMBufferDisplay( params ExecuteMEMBuffer[ ] buff ) {
            int offsetX = 144, offsetY = 2, labelWidth = 12, dataWidth=16;
            string banner = "EX/MEM";
            Console.SetCursorPosition( offsetX , offsetY );
            Console.Write( string.Format( "=={0}=={1}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , string.Empty.PadLeft( labelWidth , ' ' ) , ( banner + "[0]" ).PadLeft( dataWidth - ( ( dataWidth - banner.Length - 3 ) / 2 ) , ' ' ).PadRight( dataWidth , ' ' ) , ( banner + "[1]" ).PadLeft( dataWidth - ( ( dataWidth - banner.Length - 3 ) / 2 ) , ' ' ).PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "Branch".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.Branch.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.Branch.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "MemRead".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.MemRead.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.MemRead.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "MemWrite".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.MemWrite.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.MemWrite.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "RegWrite".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.RegWrite.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.RegWrite.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "MemToReg".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.MemToReg.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.MemToReg.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "PCBranch".PadRight( labelWidth , ' ' ) , buff[ 0 ].PCBranch.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].PCBranch.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "ALUzero".PadRight( labelWidth , ' ' ) , buff[ 0 ].Zero.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].Zero.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "ALUresult".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( buff[ 0 ].ALUResult , 0 ).ToString().PadRight( dataWidth , ' ' ) , BitConverter.ToInt32( buff[ 1 ].ALUResult , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "ReadData2".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( buff[ 0 ].ReadData2 , 0 ).ToString().PadRight( dataWidth , ' ' ) , BitConverter.ToInt32( buff[ 1 ].ReadData2 , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "RegDstAddr".PadRight( labelWidth , ' ' ) , buff[ 0 ].DEST.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].DEST.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
        }

        /// <summary>
        /// Displays the MEM/WB information
        /// </summary>
        /// <param name="buff"></param>
        internal void MEMWBBufferDisplay( params MEMWBBuffer[ ] buff ) {
            int offsetX = 144, offsetY = 18, labelWidth = 12, dataWidth=16;
            string banner = "MEM/WB";
            Console.SetCursorPosition( offsetX , offsetY );
            Console.Write( string.Format( "=={0}=={1}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , string.Empty.PadLeft( labelWidth , ' ' ) , ( banner + "[0]" ).PadLeft( dataWidth - ( ( dataWidth - banner.Length - 3 ) / 2 ) , ' ' ).PadRight( dataWidth , ' ' ) , ( banner + "[1]" ).PadLeft( dataWidth - ( ( dataWidth - banner.Length - 3 ) / 2 ) , ' ' ).PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "RegWrite".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.RegWrite.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.RegWrite.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "MemToReg".PadRight( labelWidth , ' ' ) , buff[ 0 ].vector.MemToReg.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].vector.MemToReg.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "ReadData".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( buff[ 0 ].ReadData , 0 ).ToString().PadRight( dataWidth , ' ' ) , BitConverter.ToInt32( buff[ 1 ].ReadData , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "ALUresult".PadRight( labelWidth , ' ' ) , BitConverter.ToInt32( buff[ 0 ].ALUResult , 0 ).ToString().PadRight( dataWidth , ' ' ) , BitConverter.ToInt32( buff[ 1 ].ALUResult , 0 ).ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "||{0}||{1}||{2}||" , "RegDstAddr".PadRight( labelWidth , ' ' ) , buff[ 0 ].DEST.ToString().PadRight( dataWidth , ' ' ) , buff[ 1 ].DEST.ToString().PadRight( dataWidth , ' ' ) ) );
            Console.CursorLeft = offsetX; Console.CursorTop += 1;
            Console.Write( string.Format( "=={0}=={1}=={1}==" , string.Empty.PadLeft( labelWidth , '=' ) , string.Empty.PadLeft( dataWidth , '=' ) ) );
            Console.CursorTop += 1; Console.CursorLeft = offsetX;
        }

        /// <summary>
        /// Displays the Registry/Memory information
        /// </summary>
        internal void Render() {
            //Memory and Registers
            int LeftOffset = 0, TopOffset=43;
            Console.SetCursorPosition( LeftOffset , TopOffset );
            //Memory
            int Width = 8;
            int TotalWidth = ((2 + IntWidth) * (Width+1)) + 2;
            string banner = "32-Bit Signed Representation Memory Values["+this.MEM.Size+"]";
            Console.CursorTop += 1;
            Console.Write( string.Empty.PadRight( TotalWidth , '=' ) );
            Console.CursorTop += 1; Console.CursorLeft = LeftOffset;
            Console.Write( "||" ); Console.Write( string.Empty.PadRight( TotalWidth - 4 , ' ' ) + "||" );
            Console.CursorLeft = LeftOffset + ( TotalWidth / 2 ) - ( banner.Length / 2 );
            Console.Write( banner );
            Console.CursorTop += 1; Console.CursorLeft = LeftOffset;

            Console.Write( string.Empty.PadRight( TotalWidth , '=' ) );
            Console.CursorTop += 1;
            Console.CursorLeft = LeftOffset;
            for( uint i = 0; i < this.MEM.Size; ++i ) {
                Console.Write( "||" );
                Console.Write( FormatByteArray( this.MEM[ i << 2 ] ) );
                if( ( i + 1 ) % Width == 0 ) {
                    Console.Write( "||" + ( ( i - Width + 1 ) + ".." + ( i ) + "||" ).PadLeft( IntWidth + 2 ) );
                    Console.CursorTop += 1;
                    Console.CursorLeft = LeftOffset;
                }
            }
            Console.CursorLeft = LeftOffset;
            Console.Write( string.Empty.PadRight( TotalWidth , '=' ) );
            LeftOffset = 0;
            Console.SetCursorPosition( LeftOffset , 8 );
            //Registers
            banner = "Registers[32]";
            TotalWidth = ( ( 2 + ( IntWidth + 5 ) ) ) + 2;
            Console.Write( string.Empty.PadLeft( TotalWidth , '=' ) );
            Console.CursorTop += 1; Console.CursorLeft = LeftOffset;
            Console.Write( "||" ); Console.Write( string.Empty.PadRight( TotalWidth - 4 , ' ' ) + "||" );
            Console.CursorLeft = LeftOffset + ( TotalWidth / 2 ) - ( banner.Length / 2 );
            Console.Write( banner );
            Console.CursorTop += 1; Console.CursorLeft = LeftOffset;
            Console.Write( string.Empty.PadRight( TotalWidth , '=' ) );
            Console.CursorTop += 1; Console.CursorLeft = LeftOffset;
            for( byte i = 0; i < 32; ++i ) {
                Console.Write( ( string.Format( "||{0}||{1}||" , string.Format( "${0}" , i ).PadLeft( 3 , ' ' ) , this.REG[ i ].ToString().PadLeft( IntWidth , ' ' ) ) ) );
                Console.CursorLeft = LeftOffset;
                Console.CursorTop += 1;
            }

            Console.Write( string.Empty.PadLeft( TotalWidth , '=' ) );
        }
    }
}