// Logger.cs
// Copyright Karel Kroeze, 2017-2020

using System.Diagnostics;
using Verse;

namespace FluffyManager
{
    public static class Logger
    {
        public const string identifier = "Colony Manager";

        [Conditional( "DEBUG" )]
        public static void Debug( string message )
        {
            Log.Message( identifier + " :: " + message );
        }

        [Conditional( "DEBUG_FOLLOW" )]
        public static void Follow( string message )
        {
            Log.Message( identifier + " :: " + message );
        }

        public static void Warning( string message )
        {
            Log.Warning( identifier + " :: " + message );
        }

        public static void Error( string message )
        {
            Log.Error( identifier + " :: " + message );
        }

        public static void Message( string message )
        {
            Log.Message( identifier + " :: " + message );
        }
    }
}