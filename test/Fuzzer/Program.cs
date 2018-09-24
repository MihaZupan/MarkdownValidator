/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator;
using MihaZupan.MarkdownValidator.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MihaZupan.Fuzzer
{
    public class Fuzzer
    {
        public static void Main(string[] args)
        {
            Fuzz(100000, 20000, 100000, "abAB123^-[]()#\\/*`\" .:\n\r\t", "abABc", 5);
        }

        public static void Fuzz(int iterations, int stringSizeMin, int stringSizeMax, string alphabet, string fileAlphabet, int maxFileNameLength)
        {
            MarkdownContextValidator validator = new MarkdownContextValidator(new Config(""));
            Random seedRandom = new Random();

#if RELEASE
            try
            {
#endif
                int concurrent = 4;
                int localIterations = iterations / concurrent;
                Task[] tasks = new Task[concurrent];
                for (int t = 0; t < concurrent; t++)
                {
                    Random random = new Random(seedRandom.Next());
                    tasks[t] = Task.Run(() =>
                    {
                        char[] stringChars = new string(' ', stringSizeMax).ToCharArray();
                        for (int i = 0; i < localIterations; i++)
                        {
                            if (i % 100 == 99) validator.Validate();
                            try
                            {
                                int fileNameLength = 1 + random.Next(maxFileNameLength);
                                stringChars[random.Next(fileNameLength)] = fileAlphabet[random.Next(fileAlphabet.Length)];
                                string fileName = new string(stringChars, 0, fileNameLength);

                                int stringLength = random.Next(stringSizeMin, stringSizeMax);
                                stringChars[random.Next(stringLength)] = alphabet[random.Next(alphabet.Length)];
                                string randomString = new string(stringChars, 0, stringLength);

                                if (i % 100 == 0)
                                {
                                    if (!validator.UpdateMarkdownFile(fileName, randomString))
                                        validator.AddMarkdownFile(fileName, randomString);
                                }
                                else
                                {
                                    fileName = Guid.NewGuid() + fileName;
                                    validator.AddMarkdownFile(fileName, randomString);
                                }
                            }
                            catch (ArgumentException ae) when (ae.Message.EndsWith("is not a child path of the root working directory of the context", StringComparison.Ordinal)) { }
                            catch (PathTooLongException) { }
                        }
                    });
                }
                Task.WaitAll(tasks);
#if RELEASE
            }
            catch
            {
                Console.WriteLine("Failed");
                Console.ReadLine();
            }
#endif
        }
    }
}
