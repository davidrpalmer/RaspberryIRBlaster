using System;
using System.Linq;
using System.Collections.Generic;

namespace RaspberryIRBlaster.RemoteBuilder
{
    static class ConsoleUtilities
    {
        public static void ClearInputBuffer()
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
        }

        public static bool AskYesNo(string question)
        {
            while (true)
            {
                Console.WriteLine();
                Console.Write(question);
                Console.Write(" [Y/N]");
                ClearInputBuffer();
                string input = Console.ReadLine();

                if (input.Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine();
                    return true;
                }

                if (input.Equals("n", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine();
                    return false;
                }
            }
        }

        public static int AskMultipleChoiceWithCancelAtMinusOne(string question, IEnumerable<string> choices)
        {
            return AskMultipleChoiceWithCancelAtZero(question, choices) - 1;
        }

        public static int AskMultipleChoiceWithCancelAtZero(string question, IEnumerable<string> choices)
        {
            return AskMultipleChoice(question, new string[] { "Cancel" }.Concat(choices).ToArray());
        }

        /// <returns>
        /// The choice selected by the user. First choice is 0, second choice is 1, etc.
        /// </returns>
        public static int AskMultipleChoice(string question, params string[] choices)
        {
            Console.WriteLine();
            Console.WriteLine(question);
            for (int i = 0; i < choices.Length; i++)
            {
                string number = $"[{i + 1}]";

                Console.WriteLine($"  {number.PadRight(5)}{choices[i]}");
            }
            while (true)
            {
                Console.Write(">");
                ClearInputBuffer();
                if (int.TryParse(Console.ReadLine(), out int chosen))
                {
                    chosen--;
                    if (chosen >= 0 && chosen < choices.Length)
                    {
                        return chosen;
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Enter the choice number, then press enter.");
            }
        }

        public static int AskForInteger(int min, int max)
        {
            while (true)
            {
                Console.Write(">");
                ClearInputBuffer();
                if (int.TryParse(Console.ReadLine(), out int chosen))
                {
                    if (chosen >= min && chosen <= max)
                    {
                        return chosen;
                    }
                    Console.WriteLine($"Must be a number between {min} and {max}.");
                }
                else
                {
                    Console.WriteLine("Invalid number.");
                }
            }
        }

        public static int AskForIntegerWithDefault(int min, int max, int defaultValue)
        {
            while (true)
            {
                Console.Write(">");
                ClearInputBuffer();
                string str = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(str))
                {
                    return defaultValue;
                }

                if (int.TryParse(str, out int chosen))
                {
                    if (chosen >= min && chosen <= max)
                    {
                        return chosen;
                    }
                    Console.WriteLine($"Must be a number between {min} and {max}.");
                }
                else
                {
                    Console.WriteLine("Invalid number.");
                }
                Console.WriteLine($"Leave blank to use default value of {defaultValue}.");
            }
        }
    }
}
