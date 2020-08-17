using Discord.Commands;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;

namespace DiscordTest
{
    public class Player
    {
        private readonly IUser _user;
        private readonly Game _game;
        private readonly Role _role;

        public Player(IUser user, Game game, Role role)
        {
            this._user = user;
            this._game = game;
            this._role = role;
            
        }

        public IUser User
        {
            get => this._user;
        }

        public Game Game
        {
            get => this._game;
        }

        public Role Role
        {
            get;
            set;
        }

        public Role KnownRole
        {
            get;
            set;
        }
    }
}
