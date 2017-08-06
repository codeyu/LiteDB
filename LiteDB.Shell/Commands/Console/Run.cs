﻿using System;
using System.IO;

namespace LiteDB.Shell.Commands
{
    internal class Run : ICommand
    {
        public bool IsCommand(StringScanner s)
        {
            return s.Scan(@"run\s+").Length > 0;
        }

        public void Execute(StringScanner s, Env env)
        {
            if (env.Engine == null) throw ShellException.NoDatabase();

            var filename = s.Scan(@".+").Trim();

            foreach (var line in File.ReadAllLines(filename))
            {
                env.Input.Queue.Enqueue(line);
            }
        }
    }
}