namespace CS3725.MIPSBuffer {

    using MIPSControl;
    using MIPSInstructions;
    using System;

    /// <summary>
    /// The buffer sandwiched between the Fetch and Decode stages
    /// </summary>
    public class FetchDecodeBuffer {

        /// <summary>
        /// The PC + 4 result calculated in the Fetch Stage
        /// </summary>
        public uint PC4;

        /// <summary>
        /// The Instruction fetched from the Instruction ROM
        /// </summary>
        public Instruction Instr;

        /// <summary>
        /// Constructor for a Fetch/Decode sandwich buffer
        /// </summary>
        public FetchDecodeBuffer() {
            this.PC4 = 0;
            this.Instr = 0;
        }

        /// <summary>
        /// Causes the buffer to push all the data to the next stage. Needs the target buffer to work.
        /// </summary>
        /// <param name="buff"></param>
        public void Flush( FetchDecodeBuffer Next ) {
            Next.Instr = this.Instr;
            Next.PC4 = this.PC4;
        }

        /// <summary>
        /// Passes relevant information from the side of this buffer which is
        /// in the Decode stage to the Decode/Execute sandwich buffer.
        /// </summary>
        /// <param name="buff"></param>
        public void Pass( DecodeExecuteBuffer buff ) {
            buff.Instr = this.Instr;
            buff.PC4 = this.PC4;
        }
    }

    /// <summary>
    /// The buffer sandwiched between the Decode and Execute stages
    /// </summary>
    public class DecodeExecuteBuffer {

        /// <summary>
        /// The PC + 4 calculated in the Fetch stage
        /// </summary>
        public uint PC4;

        /// <summary>
        /// The instruction from the Fetch stage
        /// </summary>
        public Instruction Instr;

        /// <summary>
        /// The byte[4] representation of the data read from rs target register
        /// </summary>
        public byte[] ReadData1;

        /// <summary>
        /// The byte[4] representation of the data read from rt target register
        /// </summary>
        public byte[] ReadData2;

        /// <summary>
        /// The byte[4] OFFSET calculated by Sign Extend
        /// </summary>
        public byte[] OFFSET;

        /// <summary>
        /// The byte value representing the RT register target
        /// </summary>
        public byte RT;

        /// <summary>
        /// The byte value representing the RD register target
        /// </summary>
        public byte RD;

        /// <summary>
        /// The Control Vector determined in the Decode stage
        /// </summary>
        public ControlVector vector;

        /// <summary>
        /// Constructor for a Decode/Execute sandwich buffer
        /// </summary>
        public DecodeExecuteBuffer() {
            this.ReadData1 = new byte[ 4 ];
            this.ReadData2 = new byte[ 4 ];
            this.OFFSET = new byte[ 4 ];
        }

        /// <summary>
        /// Passes ReadData2 and Control Vector
        /// </summary>
        /// <param name="buff"></param>
        public void Pass( ExecuteMEMBuffer buff ) {
            buff.Instr = this.Instr;
            Array.Copy( this.ReadData2 , buff.ReadData2 , 4 );
            buff.vector = this.vector;
        }

        /// <summary>
        /// Causes the buffer to push all the data to the next stage. Needs the target buffer to work.
        /// </summary>
        /// <param name="Next"></param>
        public void Flush( DecodeExecuteBuffer Next ) {
            Next.Instr = this.Instr;
            Next.PC4 = this.PC4;
            Array.Copy( this.ReadData1 , Next.ReadData1 , 4 );
            Array.Copy( this.ReadData2 , Next.ReadData2 , 4 );
            Array.Copy( this.OFFSET , Next.OFFSET , 4 );
            Next.RT = this.RT;
            Next.RD = this.RD;
            Next.vector = this.vector;
        }
    }

    /// <summary>
    /// The buffer sandwiched between the Execute and Memory stages
    /// </summary>
    public class ExecuteMEMBuffer {

        /// <summary>
        /// The instruction from the Fetch stage
        /// </summary>
        public Instruction Instr;

        /// <summary>
        /// The PCBranch value calculated in the Execute stage
        /// </summary>
        public uint PCBranch;

        /// <summary>
        /// The signal from the ALU if the result of the last operation is ZERO
        /// </summary>
        public bool Zero;

        /// <summary>
        /// The byte[4] representation of the ALUresult calculated during the Execute stage
        /// </summary>
        public byte[] ALUResult;

        /// <summary>
        /// The byte[4] representation of the ReadData2 value read from the rt target register
        /// </summary>
        public byte[] ReadData2;

        /// <summary>
        /// The byte value representation of the selected by RegDst MUXER which chose either RT or RD
        /// </summary>
        public byte DEST;

        /// <summary>
        /// The Control Vector calculated during the Decode Stage
        /// </summary>
        public ControlVector vector;

        /// <summary>
        /// Constructor for a Execute/Memory sandwich buffer
        /// </summary>
        public ExecuteMEMBuffer() {
            this.ALUResult = new byte[ 4 ];
            this.ReadData2 = new byte[ 4 ];
        }

        /// <summary>
        /// Passes DST, ALUResult, and Control Vector
        /// </summary>
        /// <param name="buff"></param>
        public void Pass( MEMWBBuffer buff ) {
            buff.Instr = this.Instr;
            buff.DEST = this.DEST;
            Array.Copy( this.ALUResult , buff.ALUResult , 4 );
            buff.vector = this.vector;
        }

        /// <summary>
        /// Causes the buffer to push all the data to the next stage. Needs the target buffer to work.
        /// </summary>
        /// <param name="Next"></param>
        public void Flush( ExecuteMEMBuffer Next ) {
            Next.Instr = this.Instr;
            Next.PCBranch = this.PCBranch;
            Next.Zero = this.Zero;
            Array.Copy( this.ALUResult , Next.ALUResult , 4 );
            Array.Copy( this.ReadData2 , Next.ReadData2 , 4 );
            Next.DEST = this.DEST;
            Next.vector = this.vector;
        }
    }

    /// <summary>
    /// The buffer sandwiched between the Memory and WriteBack stages
    /// </summary>
    public class MEMWBBuffer {

        /// <summary>
        /// The instruction from the Fetch stage
        /// </summary>
        public Instruction Instr;

        /// <summary>
        /// The byte[4] representation of the data read from memory
        /// </summary>
        public byte[] ReadData;

        /// <summary>
        /// The byte[4] representation of the ALUresult
        /// </summary>
        public byte[] ALUResult;

        /// <summary>
        /// The byte representation of the RegDst selected destination during the Execute stage
        /// </summary>
        public byte DEST;

        /// <summary>
        /// The Control Vector calculated during the Decode stage
        /// </summary>
        public ControlVector vector;

        /// <summary>
        /// Constructor for a Memory/Writeback sandwich buffer
        /// </summary>
        public MEMWBBuffer() {
            this.ReadData = new byte[ 4 ];
            this.ALUResult = new byte[ 4 ];
        }

        /// <summary>
        /// Causes the buffer to push all the data to the next stage. Needs the target buffer to work.
        /// </summary>
        /// <param name="Next"></param>
        public void Flush( MEMWBBuffer Next ) {
            Next.Instr = this.Instr;
            Array.Copy( this.ReadData , Next.ReadData , 4 );
            Array.Copy( this.ALUResult , Next.ALUResult , 4 );
            Next.DEST = this.DEST;
            Next.vector = this.vector;
        }
    }
}