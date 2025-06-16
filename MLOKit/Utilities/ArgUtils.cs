using System;
using System.Collections.Generic;

namespace MLOKit.Utilities
{
    internal static class ArgUtils
    {

        private const char _VALUE_SEPARATOR = ':';

        /**
        * Parse the arguments
        * 
        * */
        public static Dictionary<string, string> ParseArguments(IEnumerable<string> args)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string arg in args)
            {

                string[] parts = arg.Split(new char[] { _VALUE_SEPARATOR }, 2);
                if (parts.Length == 2)
                {
                    result[parts[0].ToLower().Substring(1)] = parts[1];
                }
                else
                {
                    result[parts[0].ToLower().Substring(1)] = "";
                }
            }
            return result;
        }

        /**
        * Generate module header
        * 
        * */
        public static string GenerateHeader(string module, string credential, string platform)
        {
            string output = String.Empty;
            string delim = "==================================================";
            output += "\n" + delim + "\n";
            output += "Module:\t\t" + module + "\n";
            output += "Platform:\t" + platform + "\n";
            output += "Timestamp:\t" + DateTime.Now + "\n";
            output += delim + "\n";

            return output;
        }


        /**
        * print help
        * 
        * */
        public static void HelpMe()
        {
            Console.Write("\nPlease read the README page for proper usage of the tool.\n\n");


        } // end print help method

    }

}
