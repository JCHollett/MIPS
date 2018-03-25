namespace CS3725.MIPSArchitecture {

    using System;
    using MIPSBuffer;
    using MIPSControl;
    using MIPSInstructions;
    using MIPSRegisters;
    using MIPSComponents;
    using MIPSMemory;

    /// <summary>
    /// The collection of components and modules to make a MIPS-like architecture
    /// </summary>
    public class MIPSArch {

        /// <summary>
        /// This is the Data Memory it shares the same location as the program instructions located in Fetch and Memory stages
        /// </summary>
        private DataMemory MEM;

        /// <summary>
        /// This module contains the 32 Registers located in the Decode stage
        /// </summary>
        private Registry REG;

        /// <summary>
        /// This is the Program Counter in the Fetch stage
        /// </summary>
        private uint PC;

        /// <summary>
        /// This is the readonly Instruction memory in the Fetch stage
        /// </summary>
        private IMemory InstrMEM;

        /// <summary>
        /// This is the instruction register in the Fetch stage
        /// </summary>
        private IRegister IR;

        /// <summary>
        /// This is the ALU-unit in the Execute stage
        /// </summary>
        private ALU ALUnit;

        /// <summary>
        /// This is the sign extend module in the Decode stage
        /// </summary>
        private SignExtend SGNEXT;

        /// <summary>
        /// This is the dedicated Adder in the Execute stage
        /// </summary>
        private ADD Add;

        /// <summary>
        /// This is the OP Control Unit in the Decode stage
        /// </summary>
        private OpControl Control;

        /// <summary>
        /// This is the ALU Control Unit in the Execute stage
        /// </summary>
        private ALUControl ALControl;

        /// <summary>
        /// This is the AND-gate located in the Memory stage
        /// </summary>
        private AND BranchGate;

        /// <summary>
        /// This is the First buffer; between the Fetch and Decode stages
        /// </summary>
        private FetchDecodeBuffer[] FetchBuffer;

        /// <summary>
        /// This is the Second buffer; between the Decode and Execute stages
        /// </summary>
        private DecodeExecuteBuffer[] DecodeBuffer;

        /// <summary>
        /// This is the Third buffer; between the Execute and Memory stages
        /// </summary>
        private ExecuteMEMBuffer[] ExecuteBuffer;

        /// <summary>
        /// This is the Fourth buffer; betwen the Memory and WriteBack stages
        /// </summary>
        private MEMWBBuffer[] MEMBuffer;

        /// <summary>
        /// If PC hits this value, the program ends.
        /// </summary>
        private uint MAX_PC;

        /// <summary>
        /// If this property is true, then this MIPS machine is valid and can simulate the program
        /// </summary>
        public bool Success { get { return this.MAX_PC > 0; } }

        /// <summary>
        /// This is the CUI used to render information to the console.
        /// </summary>
        private CUI CLI;

        /// <summary>
        /// The constructor for a MIPS machine. Takes Memory size and Instruction ROM size.
        /// </summary>
        /// <param name="memSize"></param>
        /// <param name="instrSize"></param>
        public MIPSArch( uint memSize , uint instrSize ) {
            PC = 0;
            MEM = new DataMemory( memSize );
            InstrMEM = new IMemory( instrSize );
            IR = new IRegister();
            REG = new Registry();
            ALUnit = new ALU();
            SGNEXT = new SignExtend();
            Add = new ADD();
            Control = new OpControl();
            ALControl = new ALUControl();
            BranchGate = new AND();
            FetchBuffer = new FetchDecodeBuffer[ ] { new FetchDecodeBuffer() , new FetchDecodeBuffer() };
            DecodeBuffer = new DecodeExecuteBuffer[ ] { new DecodeExecuteBuffer() , new DecodeExecuteBuffer() };
            ExecuteBuffer = new ExecuteMEMBuffer[ ] { new ExecuteMEMBuffer() , new ExecuteMEMBuffer() };
            MEMBuffer = new MEMWBBuffer[ ] { new MEMWBBuffer() , new MEMWBBuffer() };
            MAX_PC = new MIPSLoader( MEM , InstrMEM , REG ).Initialize();
            CLI = new CUI( ref InstrMEM , ref MEM , ref REG );
        }

        public void CUIRender() {
            try {
                CLI.PCDisplay( this.PC );
                CLI.RenderMEMREG();
                CLI.FetchDisplay( ref this.FetchBuffer[ 0 ] , BranchGate.Output , PC , ExecuteBuffer[ 1 ].PCBranch );
                CLI.DecodeDisplay( ref this.FetchBuffer[ 1 ] , ref this.DecodeBuffer[ 0 ] , MEMBuffer[ 1 ].vector.RegWrite );
                CLI.ExecuteDisplay( ref this.DecodeBuffer[ 1 ] , ref this.ExecuteBuffer[ 0 ] );
                CLI.MEMDisplay( ref this.ExecuteBuffer[ 1 ] , ref this.MEMBuffer[ 0 ] , BranchGate.Output );
                CLI.WBDisplay( ref this.MEMBuffer[ 1 ] );
                CLI.FetchDecodeBufferDisplay( FetchBuffer[ 0 ] , FetchBuffer[ 1 ] );
                CLI.DecodeExecuteBufferDisplay( DecodeBuffer[ 0 ] , DecodeBuffer[ 1 ] );
                CLI.ExecuteMEMBufferDisplay( ExecuteBuffer[ 0 ] , ExecuteBuffer[ 1 ] );
                CLI.MEMWBBufferDisplay( MEMBuffer[ 0 ] , MEMBuffer[ 1 ] );
            } catch { /*This should be error proofed*/ };
        }

        /// <summary>
        /// This returns a boolean which represents the failure, success, completion or advancing cycles of this mips machine.
        /// An advancing cycle causes the data to proceed a singular cycle.
        /// A completion cycle returns false at the end
        /// The Success property must by true for this Method to be used successfully.
        /// </summary>
        /// <returns></returns>
        public bool Cycle() {
            if( !Success ) return false;
            //FLUSHING
            FetchBuffer[ 0 ].Flush( FetchBuffer[ 1 ] );
            DecodeBuffer[ 0 ].Flush( DecodeBuffer[ 1 ] );
            ExecuteBuffer[ 0 ].Flush( ExecuteBuffer[ 1 ] );
            MEMBuffer[ 0 ].Flush( MEMBuffer[ 1 ] );
            //END FLUSHING
            //PASSING
            FetchBuffer[ 1 ].Pass( DecodeBuffer[ 0 ] );
            DecodeBuffer[ 1 ].Pass( ExecuteBuffer[ 0 ] );
            ExecuteBuffer[ 1 ].Pass( MEMBuffer[ 0 ] );
            //END PASSING
            //Writeback STAGE
            REG.WriteRegister = MEMBuffer[ 1 ].DEST;
            REG.WriteData = MUXER<byte[ ]>.Select( MEMBuffer[ 1 ].ALUResult , MEMBuffer[ 1 ].ReadData , MEMBuffer[ 1 ].vector.MemToReg );

            //MEM STAGE
            BranchGate.A = ExecuteBuffer[ 1 ].vector.Branch;
            BranchGate.B = ExecuteBuffer[ 1 ].Zero;
            MEM.WriteAddress = MEM.ReadAddress = BitConverter.ToUInt32( ExecuteBuffer[ 1 ].ALUResult , 0 );
            MEM.WriteData = ExecuteBuffer[ 1 ].ReadData2;
            MEM.MemRead = ExecuteBuffer[ 1 ].vector.MemRead;
            MEM.MemWrite = ExecuteBuffer[ 1 ].vector.MemWrite;
            MEMBuffer[ 0 ].ReadData = MEM.ReadData;

            //EXECUTE STAGE
            Add.A = DecodeBuffer[ 1 ].PC4;
            Add.B = BitConverter.ToInt32( DecodeBuffer[ 1 ].OFFSET , 0 ) << 2;
            ALControl.ALUOp0 = DecodeBuffer[ 1 ].vector.ALUOp0;
            ALControl.ALUOp1 = DecodeBuffer[ 1 ].vector.ALUOp1;
            ALControl.FUNCT = ( ( byte )( BitConverter.ToChar( DecodeBuffer[ 1 ].OFFSET , 0 ) & 0x3F ) );
            ALUnit.ALU0 = DecodeBuffer[ 1 ].ReadData1;
            ALUnit.ALU1 = MUXER<byte[ ]>.Select( DecodeBuffer[ 1 ].ReadData2 , DecodeBuffer[ 1 ].OFFSET , DecodeBuffer[ 1 ].vector.ALUSrc );
            ALUnit.Vector = ALControl.Output;
            ExecuteBuffer[ 0 ].PCBranch = ( uint )Add.Output;
            ExecuteBuffer[ 0 ].DEST = MUXER<byte>.Select( DecodeBuffer[ 1 ].RT , DecodeBuffer[ 1 ].RD , DecodeBuffer[ 1 ].vector.RegDst );
            ExecuteBuffer[ 0 ].ALUResult = ALUnit.ALUResult;
            ExecuteBuffer[ 0 ].Zero = ALUnit.Zero;

            //DECODE STAGE
            REG.Input = FetchBuffer[ 1 ].Instr;
            REG.RegWrite = MEMBuffer[ 1 ].vector.RegWrite;
            Control.Input = FetchBuffer[ 1 ].Instr;
            SGNEXT.Input = REG.Input.OFFSET;
            DecodeBuffer[ 0 ].vector = Control.Output;
            DecodeBuffer[ 0 ].ReadData1 = REG.ReadData1;
            DecodeBuffer[ 0 ].ReadData2 = REG.ReadData2;
            DecodeBuffer[ 0 ].OFFSET = SGNEXT.Output;
            DecodeBuffer[ 0 ].RT = FetchBuffer[ 1 ].Instr.RT;
            DecodeBuffer[ 0 ].RD = FetchBuffer[ 1 ].Instr.RD;

            //FETCH STAGE
            CLI.PCDisplay( this.PC );
            IR = InstrMEM[ PC ];
            PC = MUXER<uint>.Select( PC + 4 , ExecuteBuffer[ 1 ].PCBranch , BranchGate.Output );
            FetchBuffer[ 0 ].PC4 = PC;
            FetchBuffer[ 0 ].Instr = IR.Output;

            return PC <= MAX_PC;
        }
    }
}