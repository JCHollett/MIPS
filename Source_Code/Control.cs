using CS3725.MIPSInstructions;

namespace CS3725.MIPSControl {

    /// <summary>
    /// The Control vector calculated during Decode stage
    /// </summary>
    public struct ControlVector {
        #region EXECUTION STAGE

        /// <summary>
        /// RegDst signal
        /// </summary>
        public bool RegDst;

        /// <summary>
        /// ALUop0 signal
        /// </summary>
        public bool ALUOp0;

        /// <summary>
        /// ALUop1 signal
        /// </summary>
        public bool ALUOp1;

        /// <summary>
        /// ALUsrc signal
        /// </summary>
        public bool ALUSrc;

        #endregion EXECUTION STAGE
        #region MEMORY STAGE

        /// <summary>
        /// Branch signal
        /// </summary>
        public bool Branch;

        /// <summary>
        /// MemRead signal
        /// </summary>
        public bool MemRead;

        /// <summary>
        /// MemWrite signal
        /// </summary>
        public bool MemWrite;

        #endregion MEMORY STAGE
        #region WRITEBACK STAGE

        /// <summary>
        /// RegWrite signal
        /// </summary>
        public bool RegWrite;

        /// <summary>
        /// MemToReg signal
        /// </summary>
        public bool MemToReg;

        #endregion WRITEBACK STAGE

        /// <summary>
        /// The constructor for a Control Vector
        /// </summary>
        /// <param name="instr"></param>
        public ControlVector( Instruction instr ) {
            //NOP-Format
            if( instr.Data == 0 ) {
                this.RegDst = this.ALUSrc = this.Branch = this.MemRead = this.MemWrite = this.RegWrite = this.MemToReg = false;
                this.ALUOp0 = ( this.ALUOp1 = true );
            } else
                switch( instr.OpCode ) {
                    //R-Format
                    case 0:
                        this.RegDst = true;
                        this.ALUOp1 = true;
                        this.ALUOp0 = false;
                        this.ALUSrc = false;
                        this.Branch = false;
                        this.MemRead = false;
                        this.MemWrite = false;
                        this.RegWrite = true;
                        this.MemToReg = false;
                        break;
                    //lw
                    case 35:
                        this.RegDst = false;
                        this.ALUOp1 = false;
                        this.ALUOp0 = false;
                        this.ALUSrc = true;
                        this.Branch = false;
                        this.MemRead = true;
                        this.MemWrite = false;
                        this.RegWrite = true;
                        this.MemToReg = true;
                        break;
                    //sw
                    case 43:
                        this.RegDst = false;
                        this.ALUOp0 = false;
                        this.ALUOp1 = false;
                        this.ALUSrc = true;
                        this.Branch = false;
                        this.MemRead = false;
                        this.MemWrite = true;
                        this.RegWrite = false;
                        this.MemToReg = false;
                        break;
                    //beq
                    case 4:
                        this.RegDst = false;
                        this.ALUOp1 = false;
                        this.ALUOp0 = true;
                        this.ALUSrc = false;
                        this.Branch = true;
                        this.MemRead = false;
                        this.MemWrite = false;
                        this.RegWrite = false;
                        this.MemToReg = false;
                        break;
                    //Any other unsupported operation is taken as a NOP by default
                    default:
                        this.RegDst = this.ALUSrc = this.Branch = this.MemRead = this.MemWrite = this.RegWrite = this.MemToReg = false;
                        this.ALUOp0 = ( this.ALUOp1 = true );
                        break;
                }
        }
    }

    /// <summary>
    /// The Op Control unit
    /// This module is for taking a UInt32 value and converting it into an instruction
    /// </summary>
    public class OpControl {

        /// <summary>
        /// Stores a temporary instruction to be passed at a later time
        /// </summary>
        private Instruction instr;

        /// <summary>
        /// Takes a UInt32 value implicitly and converts it into an Instruction
        /// </summary>
        public Instruction Input { set { this.instr = value; } }
        /// <summary>
        /// Creates a copy of the instruction and passes it out of this module
        /// </summary>
        public ControlVector Output { get { return new ControlVector( this.instr ); } }
    }

    /// <summary>
    /// An ALU Control vector
    /// </summary>
    public struct ALUVector {

        /// <summary>
        /// The value of the aluoperation stored by this vector
        /// </summary>
        private byte aluop;

        /// <summary>
        /// The property which returns the aluop variable public visibility
        /// </summary>
        public byte OPVal {
            get {
                return this.aluop;
            }
        }

        /// <summary>
        /// The constructor for an ALU Vector
        /// Takes an op code and funct value from the UInt32 based instruction
        /// </summary>
        /// <param name="op"></param>
        /// <param name="funct"></param>
        public ALUVector( byte op , byte funct ) {
            switch( op ) {
                //ADD
                case 0x0:
                    this.aluop = 0x2;
                    break;

                //SUB
                case 0x1:

                    this.aluop = 0x6;
                    break;

                case 0x2:
                    switch( funct ) {
                        //add
                        case 0x20:
                            this.aluop = 0x2;
                            break;
                        //sub
                        case 0x22:
                            this.aluop = 0x6;
                            break;
                        //
                        case 0x24:
                            this.aluop = 0x0;
                            break;

                        case 0x25:
                            this.aluop = 0x1;
                            break;

                        case 0x2A:
                            this.aluop = 0x7;
                            break;

                        default:
                            this.aluop = 0x0;
                            break;
                    }
                    break;

                default:
                    this.aluop = 0x8;
                    break;
            }
        }
    }

    /// <summary>
    /// The ALU Control unit
    /// This module is for taking an op code and funct value and the two ALUop0 and ALUop1 signals and storing them
    /// The output takes these values and converts them into an ALU Control Vector
    /// </summary>
    public class ALUControl {

        /// <summary>
        /// The Op code stored in this control
        /// </summary>
        private byte opval;

        /// <summary>
        /// The funct value stored in this control
        /// </summary>
        private byte funct;

        /// <summary>
        /// The funct INPUT line from the SIGN EXTEND output in the Decode Stage
        /// </summary>
        public byte FUNCT { set { this.funct = value; } }
        /// <summary>
        /// The ALUop0 signal input line
        /// </summary>
        public bool ALUOp0 {
            get { return ( opval & 0x1 ) != 0; }
            set { opval &= 0x2; opval |= ( byte )( value ? 0x1 : 0x0 ); }
        }
        /// <summary>
        /// The ALUop1 signal input line
        /// </summary>
        public bool ALUOp1 {
            get { return ( opval & 0x2 ) != 0; }
            set { opval &= 0x1; opval |= ( byte )( value ? 0x2 : 0x0 ); }
        }
        /// <summary>
        /// The ALUvector output line
        /// </summary>
        public ALUVector Output {
            get {
                return new ALUVector( this.opval , this.funct );
            }
        }
    }
}