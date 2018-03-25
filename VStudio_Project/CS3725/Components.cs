using CS3725.MIPSControl;
using System;

namespace CS3725.MIPSComponents {

    /// <summary>
    /// The ALU used to perform various math operations in this MIPS architecture
    /// </summary>
    public class ALU {
        /// <summary>
        /// Sending an ALU vector to this property causes the ALU to function.
        /// Any information that has been stored prior to this is used in calculation.
        /// It is important this information is sent before the vector.
        /// </summary>
        public ALUVector Vector {
            set {
                switch( value.OPVal ) {
                    case 0x0:
                        //AND
                        this.result = this.alu0 & this.alu1;
                        break;

                    case 0x1:
                        //OR
                        this.result = this.alu0 | this.alu1;
                        break;

                    case 0x2:
                        //ADD
                        this.result = this.ADD( this.alu0 , this.alu1 );
                        break;

                    case 0x3:
                        //??
                        break;

                    case 0x4:
                        //??
                        break;

                    case 0x5:
                        //??
                        break;

                    case 0x6:
                        //SUB
                        this.result = this.SUB( this.alu0 , this.alu1 );
                        break;

                    case 0x7:
                        //SET ON LESS THAN

                        break;
                }
                this.Zero = this.result == 0;
            }
        }

        /// <summary>
        /// This is the ALU zero output
        /// </summary>
        public bool Zero;

        /// <summary>
        /// This is the ADD function for this unit
        /// </summary>
        private Func<int,int,int> ADD = (x,y) => x+y;

        /// <summary>
        /// This is the SUB function for this unit.
        /// </summary>
        private Func<int,int,int> SUB = (x,y) => x-y;

        /// <summary>
        /// This is the resulting value computed by the ALU
        /// </summary>
        private dynamic result;

        /// <summary>
        /// This is the first input to the ALU. It's source is ReadData1.
        /// </summary>
        private dynamic alu0;

        /// <summary>
        /// This is the second input to the ALU. It's source is either ReadData2 or SignExtend OFFSET
        /// </summary>
        private dynamic alu1;

        /// <summary>
        /// Constructor for an ALU with default values to prevent errors
        /// </summary>
        public ALU() {
            this.result = 0;
            this.alu0 = 0;
            this.alu1 = 0;
        }

        /// <summary>
        /// This is what is what is the actual output used by the running program.
        /// All values computed are converted into byte[] format so that it can be more abstractly transfered throughout the program.
        /// </summary>
        public byte[ ] ALUResult {
            get { return BitConverter.GetBytes( this.result ); }
        }
        /// <summary>
        /// This is the input for the ALU0. From ReadData1 only.
        /// Any values sent to this ALU0 are assumed to be Int32 types.
        /// </summary>
        public byte[ ] ALU0 {
            set {
                alu0 = BitConverter.ToInt32( value , 0 );
            }
        }
        /// <summary>
        /// This is the input for the ALU1. From ReadData2 or OFFSET.
        /// Any values sent to this ALU1 are assumed to be Int32 types.
        /// </summary>
        public byte[ ] ALU1 {
            set {
                alu1 = BitConverter.ToInt32( value , 0 );
            }
        }
    }

    /// <summary>
    /// The sign extend component
    /// </summary>
    public struct SignExtend {

        /// <summary>
        /// The byte[4] value of the extended input value
        /// </summary>
        private byte[] extended;

        /// <summary>
        /// The original input value
        /// </summary>
        private short original;

        /// <summary>
        /// The input line for this Component
        /// </summary>
        public short Input {
            set {
                this.original = value;
                this.extended = BitConverter.GetBytes( ( value << 16 ) >> 16 );
            }
        }
        /// <summary>
        /// The byte[4] output line for this component
        /// </summary>
        public byte[ ] Output {
            get {
                return this.extended;
            }
        }

        /// <summary>
        /// Constructor for a Sign Extend component
        /// </summary>
        /// <param name="input"></param>
        public SignExtend( short input ) {
            this.original = input;
            this.extended = BitConverter.GetBytes( ( input << 16 ) >> 16 );
        }

        /// <summary>
        /// Implicit cast for Sign Extending a short value
        /// </summary>
        /// <param name="input"></param>
        public static implicit operator SignExtend( short input ) {
            return new SignExtend( input );
        }
    }

    /// <summary>
    /// Multiplexor Component; 2-Input 1-output
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MUXER<T> {

        /// <summary>
        /// Multiplexor component Function. IF (arg3 == true) THEN OUTPUT arg2 ELSE OUTPUT arg1;
        /// <para>arg1 = x</para>
        /// <para>arg2 = y</para>
        /// <para>arg3 = z</para>
        /// </summary>
        public static Func<T , T , Boolean , T> Select = ( T x , T y , Boolean z ) => z ? y : x;
    }

    /// <summary>
    /// ADD component; 2-Input, 1-Output
    /// </summary>
    public class ADD {

        /// <summary>
        /// Input A
        /// </summary>
        public dynamic A;

        /// <summary>
        /// Input B
        /// </summary>
        public dynamic B;

        /// <summary>
        /// Result of A + B
        /// </summary>
        public dynamic Output { get { return A + B; } }
    }

    /// <summary>
    /// AND component; 2-Input, 1-Output
    /// </summary>
    public class AND {

        /// <summary>
        /// Signal A
        /// </summary>
        public bool A;

        /// <summary>
        /// Signal B
        /// </summary>
        public bool B;

        /// <summary>
        /// Result of A && B
        /// </summary>
        public bool Output { get { return A && B; } }
    }

    /// <summary>
    /// OR component; 2-Input, 1-Output
    /// </summary>
    public class OR {

        /// <summary>
        /// Signal A
        /// </summary>
        public bool A;

        /// <summary>
        /// SIgnal B
        /// </summary>
        public bool B;

        /// <summary>
        /// Result of A || B
        /// </summary>
        public bool Output { get { return A || B; } }
    }
}