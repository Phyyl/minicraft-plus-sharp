using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static MinicraftPlusSharp.Networking.InputType;

namespace MinicraftPlusSharp.Networking
{
    public interface MinicraftProtocol
    {
        const int PORT = 4225;

        void EndConnection();
        bool IsConnected();
    }

    public enum InputType
    {
        INVALID, PING, USERNAMES, LOGIN, GAME, INIT, LOAD, TILES, ENTITIES, TILE, ENTITY, PLAYER, MOVE, ADD, REMOVE, DISCONNECT, SAVE, NOTIFY, INTERACT, PUSH, PICKUP, CHESTIN, CHESTOUT, ADDITEMS, BED, POTION, HURT, DIE, RESPAWN, DROP, STAMINA, SHIRT, STOPFISHING
    }

    public static class InputTypes
    {
        public static readonly InputType[] values = Enum.GetValues<InputType>();
        public static readonly InputType[] serverOnly = new[] { INIT, TILES, ENTITIES, ADD, REMOVE, HURT, GAME, ADDITEMS, STAMINA, STOPFISHING };
        public static readonly InputType[] entityUpdates = new[] { ENTITY, ADD, REMOVE };
        public static readonly InputType[] tileUpdates = new[] { TILE };

        public static InputType? GetInputType(char idxChar)
        {
            int idx = idxChar - 1;

            if (idx < values.Length && idx >= 0)
            {
                return values[idx];
            }
            else
            {
                Console.Error.WriteLine($"Communication Error: Socket data has an invalid input type: {idx}");
                return null;
            }
        }
    }

}
