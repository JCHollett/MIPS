using System;

namespace CS3725 {

    internal class Program {

        private static void Main( string[ ] args ) {
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetWindowSize( ( int )( Console.LargestWindowWidth ) , ( int )( Console.LargestWindowHeight ) );
            Console.SetWindowPosition( 0 , 0 );
            MIPSArch mips;
            try {
                mips = new MIPSArch( 64 , 128 );
                //If the program loaded successfully
                if( mips.Success ) {
                    Console.SetCursorPosition( 0 , 0 );
                    Console.Write( string.Empty.PadRight( CUI.LastLength + 3 , ' ' ) );
                    do {
                        //Render the Memory and Registers
                        mips.CLI.Render();
                        Console.SetCursorPosition( 0 , 0 );
                        Console.Write( "Press any key to advance MIPS output..." );
                        Console.ReadKey( true );
                        //Cycle renders the rest of the data on the screen
                    } while( mips.Cycle() );
                    Console.SetCursorPosition( 0 , 0 );
                    Console.Write( "Mips simulation complete! Press any key to quit..." );
                } else {
                    Console.WriteLine( "Press any key to quit..." );
                }
            } catch( Exception ) {
                Console.WriteLine( "MIPS File Not found" );
                Console.WriteLine( "Press any key to quit..." );
            }

            Console.ReadKey( true );
        }
    }
}