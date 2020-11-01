using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Atavism
{
    public delegate void CommandHandler(string args);

    public class AtavismCommand
    {

        static Dictionary<string, CommandHandler> _commandHandlers = new Dictionary<string, CommandHandler>();

        public static void HandleCommand(string message)
        {
            //This is the standard implementation of HandleCommand for handling commands entered on the client.
            AtavismLogger.LogDebugMessage("HandleCommand: " + message);
            if (message.Length == 0)
                return;
            if (!message.StartsWith("/"))
                message = "/say " + message;
            // Handle some client side commands
            string[] tokens = message.Split(' ');

            if (tokens.Length <= 0)
                return;
            string args = "";
            //for (int i = 0; i < tokens.Length-1; i++) {
            //	args[i] = tokens[i+1];
            //}
            if (message.Length > tokens[0].Length)
                args = message.Substring(tokens[0].Length + 1);
            string command = tokens[0].Substring(1);
            AtavismLogger.LogDebugMessage("num args: " + tokens.Length + " with command: " + command);
            if (_commandHandlers.ContainsKey(command))
            {
                // We have a local handler for this command on the client.
                CommandHandler function = _commandHandlers[command];
                try
                {
                    function(args);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning("Failed to run command handler " + command + " for command line: " + message);
                    UnityEngine.Debug.LogWarning("Exception: " + e);
                }
            }
            else
            {
                // This command is not handled on the client.  Send it to the server.
                long target = ClientAPI.GetTargetOid();
                if (target == -1)
                    target = ClientAPI.GetPlayerOid();
                SendCommandToServer(message, target);
            }
        }

        public static void SendCommandToServer(string command, long targetOid)
        {
            CommandMessage commandMessage = new CommandMessage();
            commandMessage.ObjectId = targetOid;
            commandMessage.Command = command;
            AtavismNetworkHelper.Instance.SendMessage(commandMessage);
            AtavismLogger.LogDebugMessage("Sending command to server");
        }

        public static void RegisterCommandHandler(string command, CommandHandler function)
        {
            // Add the command and method to our dispatch table.
            // This will be used for the HandleCommand method, as well as for /help.
            _commandHandlers.Add(command, function);
        }

    }
}