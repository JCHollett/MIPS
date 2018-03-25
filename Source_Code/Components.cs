using System;

namespace CS3725.MIPSComponents {

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