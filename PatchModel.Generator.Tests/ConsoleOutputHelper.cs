﻿using System;
using Xunit.Abstractions;

namespace PatchModel.Generator.Tests
{
    internal sealed class ConsoleOutputHelper : ITestOutputHelper
    {
        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
