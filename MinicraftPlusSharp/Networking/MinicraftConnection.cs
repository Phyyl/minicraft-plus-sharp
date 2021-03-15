using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Items;
using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Networking
{
    public abstract class MinicraftConnection : JavaThread, MinicraftProtocol
    {
        private StreamWriter @out;
        private StreamReader @in;
        private TcpClient socket;

        protected MinicraftConnection(string threadName, TcpClient socket)
            : base(threadName)
        {
            this.socket = socket;

            if (socket is null)
            {
                return;
            }

            try
            {
                @in = new(socket.GetStream());
                @out = new(socket.GetStream())
                {
                    AutoFlush = true
                };
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine("Failed to initialize i/o streams for socket:");
                ex.PrintStackTrace();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("CONNECTION ERROR: null socket, cannot initialize i/o streams...");
                ex.PrintStackTrace();
            }
        }

        protected override void ThreadMain()
        {
            if (Game.debug)
            {
                Console.WriteLine("Starting " + this);
            }

            StringBuilder currentData = new();

            if (@in is null || @out is null)
            {
                return;
            }

            while (IsConnected())
            {
                int read = -2;

                try
                {
                    read = @in.Read();
                }
                catch (IOException ex)
                {
                    Console.Error.WriteLine(this + " had a problem reading its input stream (will continue trying): " + ex.Message);
                    ex.PrintStackTrace();
                }

                //TODO: check if end of stream is 0 in .NET
                if (read < 0)
                {
                    if (Game.debug)
                    {
                        Console.WriteLine(this + " reached end of input stream.");
                    }
                    break;
                }

                if (read > 0)
                {
                    currentData.Append((char)read);
                }
                else if (currentData.Length > 0)
                {
                    InputType? inType = InputTypes.GetInputType(currentData[0]);

                    if (!inType.HasValue)
                    {
                        Console.Error.WriteLine("SERVER: invalid packet received; input type is not valid");
                    }
                    else
                    {
                        ParsePacket(inType.Value, currentData.ToString(1, currentData.Length - 1));
                    }
                }


            }
        }

        protected int GetConnectedPort()
        {
            return ((IPEndPoint)socket.Client.LocalEndPoint).Port;
        }

        protected abstract bool ParsePacket(InputType inType, string data);

        protected void sendData(InputType inType, string data)
        {
            if (socket == null)
            {
                return;
            }

            if (Game.packet_debug && Game.IsConnectedClient())
            {
                Console.WriteLine("Sent:" + inType.ToString() + ", " + data);
            }

            char inTypeChar = (char)(inType + 1);

            if (data.Contains("\0"))
            {
                Console.Error.WriteLine("WARNING from " + this + ": data to send contains a null character. Not sending data.");
            }
            else
            {
                @out.Write(inTypeChar + data + '\0');
                @out.Flush();
            }
        }

        public static string stringToInts(string str, int maxLength)
        {
            int[] chars = new int[Math.Min(str.Length, maxLength)];

            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = (int)str[i];
            }

            return string.Join(", ", chars);
        }

        // there are a couple methods that are identical in both a server thread, and the client, so I'll just put them here.

        public void sendNotification(string  note, int notetime)
        {
            sendData(InputType.NOTIFY, notetime + ";" + note);
        }

        public void sendPotionEffect(PotionType type, bool addEffect)
        {
            sendData(InputType.POTION, addEffect + ";" + type.ordinal);
        }

        public void EndConnection()
        {
            if (socket is not null && socket.Connected)
            {
                if (Game.debug)
                {
                    Console.WriteLine("Closing socket and ending connection for: " + this);
                }

                sendData(InputType.DISCONNECT, "");

                try
                {
                    socket.Close();
                }
                catch // ignored
                {
                }
            }
        }

        public bool IsConnected()
        {
            return socket != null && socket.Connected;
        }
    }
}