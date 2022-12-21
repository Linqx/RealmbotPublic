using System;
using System.Collections.Generic;
using BotTools;

namespace BotProgram {
    public abstract class ModularProgram {

        #region Static

        private static bool _running = true;
        private static ModularProgram[] _programs;
        private static UpdateTicker[] _tickers;

        public static bool Local;

        public static void FatalError(Exception e) {
            if (e != null)
                Logger.Log("FatalError", e, ConsoleColor.Red);
            else
                Logger.Log("FatalError", Environment.StackTrace, ConsoleColor.Red);

            _running = false;
        }

        /// <summary>
        /// Runs a modular set of programs
        /// </summary>
        /// <param name="local">Determines if this configuration is local</param>
        /// <param name="programTypes">The programs to include</param>
        public static void Run(bool local, params Type[] programTypes) {
            Local = local;

            _programs = new ModularProgram[programTypes.Length];
            _tickers = new UpdateTicker[programTypes.Length];
            Dictionary<string, ModularProgram> commandHooks = new Dictionary<string, ModularProgram>();

            Logger.Log("ModularProgram", "Running with included programs:");

            for (int i = 0; i < _programs.Length; i++) {
                Type type = programTypes[i];
                try {
                    ModularProgram program = (ModularProgram) Activator.CreateInstance(type);
                    _programs[i] = program;

                    commandHooks[program.ModuleName.ToLower()] = program;
                    Logger.Log("ModularProgram", program.ModuleName);

                    program.PreStart();

                    UpdateTicker ticker = new UpdateTicker(program.FPS, program.Update);
                    _tickers[i] = ticker;
                }
                catch (Exception e) // Invalid type
                {
                    Logger.Log("ModularProgram", "Failed to load: " + type.Name + "\n" + e);
                }
            }

            for (int i = 0; i < _programs.Length; i++)
                if (_programs[i] != null) {
                    _tickers[i].Start();
                    _programs[i].Start();
                }

            while (_running) {
                if (!_running) break;

                string[] split = Console.ReadLine().Split(' ');
                string name = split[0];

                if (name == "stop") {
                    // Stop the programs
                    _running = false;
                }
                else if (split.Length > 1 && commandHooks.ContainsKey(name)) {
                    string command = split[1];
                    string[] args = new string[split.Length - 2];
                    if (split.Length > 2) Array.Copy(split, 2, args, 0, args.Length);
                    commandHooks[name].Command(command, args);
                }
                else {
                    Logger.Log("ModularProgram", "Invalid command: " + name);
                }
            }

            for (int i = 0; i < _programs.Length; i++) {
                Logger.Log("ModularProgram", "Stopping " + _programs[i].ModuleName);
                if (_programs[i] != null) _programs[i].Stop();
            }

            Logger.Log("ModularProgram", "Programs stopped");

            Console.ReadLine();
        }

        #endregion

        #region Class

        public virtual string ModuleName => "ModularProgram";

        public int FPS { get; protected set; } = 5;

        /// <summary>
        /// Called before <see cref="Start"/>, initialize components here
        /// </summary>
        protected abstract void PreStart();

        /// <summary>
        /// Start the execution of the program
        /// </summary>
        protected abstract void Start();

        /// <summary>
        /// Stops the execution of the <see cref="ModularProgram"/>
        /// </summary>
        protected abstract void Stop();

        /// <summary>
        /// Processes a command sent through the console to this program
        /// </summary>
        /// <param name="command">The name of the command sent</param>
        /// <param name="args">The arguments sent with the command</param>
        protected abstract void Command(string command, string[] args);

        protected abstract void Update(int time, int dt);

        #endregion

    }
}