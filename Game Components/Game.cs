using Discord.Commands;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Discord;
using Discord.WebSocket;
using System.Linq;
using System.IO;


namespace DiscordTest
{
    public class Game
    {
        public readonly string discordRole = "in play";
        public readonly IDictionary<IUser, Player> _players;
        public readonly IUser Storyteller;
        private static readonly Category townsfolk = new Category("Townsfolk");
        private static readonly Category outsider = new Category("Outsider");
        private static readonly Category minion = new Category("Minion");
        private static readonly Category demon = new Category("Demon");
        private static readonly List<Category> categories = new List<Category>
            {
                townsfolk,
                outsider,
                minion,
                demon,
            };
        List<Role> allRoles = new List<Role>();
        Dictionary<string, Role> roleNames = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);
        private readonly string _script;

        public Game(IUser storyteller, string script)
        {
            this._players = new Dictionary<IUser, Player>();
            this.Storyteller = storyteller;
            this._script = script;
        }

        public IReadOnlyCollection<Player> Players
        {
            get => (IReadOnlyCollection<Player>)this._players.Values;
        }

        public async Task Start(ICommandContext ctx)
        {
            IReadOnlyCollection<IGuildUser> users = await ctx.Guild.GetUsersAsync();
            IRole inPlayRole = ctx.Guild.Roles.FirstOrDefault(role => role.Name.Equals(discordRole, StringComparison.OrdinalIgnoreCase));

            if (inPlayRole == null)
            {
                throw new InvalidOperationException($"No {inPlayRole.Name} role found.");
            }

            foreach (IGuildUser user in users)
            {
                if (user.RoleIds.Contains(inPlayRole.Id) && user.Status == UserStatus.Online)
                {
                    Player newplayer = new Player(user, this, null);
                    _players.Add(user, newplayer);
                }
            }

            if (_players.Count < 5)
            {
                throw new InvalidDataException("Too few players!");
            }
            if (_players.Count > 15)
            {
                throw new InvalidDataException($"Too many players! Please ask {15 - _players.Count} to remove the {inPlayRole.Name} role until setup is done; they can then enter as travelers.");
            }

            demon.TotalCount = 1;

            if (_players.Count > 14)
            {
                townsfolk.TotalCount = 9;
                outsider.TotalCount = 2;
                minion.TotalCount = 3;
            }
            else
            {
                switch (_players.Count)
                {
                    case 5:
                        townsfolk.TotalCount = 3;
                        outsider.TotalCount = 0;
                        minion.TotalCount = 1;
                        break;
                    case 6:
                        townsfolk.TotalCount = 3;
                        outsider.TotalCount = 1;
                        minion.TotalCount = 1;
                        break;
                    case 7:
                        townsfolk.TotalCount = 5;
                        outsider.TotalCount = 0;
                        minion.TotalCount = 1;
                        break;
                    case 8:
                        townsfolk.TotalCount = 5;
                        outsider.TotalCount = 1;
                        minion.TotalCount = 1;
                        break;
                    case 9:
                        townsfolk.TotalCount = 5;
                        outsider.TotalCount = 2;
                        minion.TotalCount = 1;
                        break;
                    case 10:
                        townsfolk.TotalCount = 7;
                        outsider.TotalCount = 0;
                        minion.TotalCount = 2;
                        break;
                    case 11:
                        townsfolk.TotalCount = 7;
                        outsider.TotalCount = 1;
                        minion.TotalCount = 2;
                        break;
                    case 12:
                        townsfolk.TotalCount = 7;
                        outsider.TotalCount = 2;
                        minion.TotalCount = 2;
                        break;
                    case 13:
                        townsfolk.TotalCount = 9;
                        outsider.TotalCount = 0;
                        minion.TotalCount = 3;
                        break;
                    case 14:
                        townsfolk.TotalCount = 9;
                        outsider.TotalCount = 1;
                        minion.TotalCount = 3;
                        break;
                    default:
                        break;
                }
            }
        }
        public void Set(string[] command)
        {
            allRoles.Clear();
            roleNames.Clear();
            foreach (Category c in categories) { c.Count = c.TotalCount; }
            
            switch (this._script)
            {
                case "tb":
                    AddTB();
                    break;
                case "bmr":
                    AddBMR();
                    break;
                case "sav":
                    AddSAV();
                    break;
                default:
                    break;
            }

            foreach (Role role in allRoles) { roleNames.Add(role.Name, role); }

            List<Role> mandatoryRoles = new List<Role>();
            if (command.Length > 0)
            {
                foreach (string role in command)
                {
                    if (roleNames.TryGetValue(role, out Role mandatoryRole))
                    {
                        mandatoryRoles.Add(mandatoryRole);
                    }
                    else
                    {
                        throw new InvalidDataException($"{role} isn't a valid role!");
                    }
                }
            }
            List<Role> rolesInPlay = new List<Role>();
            foreach (Role mandatoryrole in mandatoryRoles.ToList())
            {
                if (allRoles.Contains(mandatoryrole))
                {
                    mandatoryrole.Category.Count--;
                    if (mandatoryrole.Category.Count < 0)
                    {
                        throw new InvalidDataException($"Too few {mandatoryrole.Category.Name} players to include that mandatory role!");
                    }
                    else
                    {
                        rolesInPlay.Add(mandatoryrole);
                        allRoles.Remove(mandatoryrole);
                        mandatoryRoles.Remove(mandatoryrole);
                    }
                }
            }
            var random = new Random();
            foreach (Category c in categories)
            {
                List<Role> l = allRoles.FindAll(r => r.Category == c);
                while (c.Count > 0)
                {
                    Role selected = l[random.Next(l.Count)];
                    rolesInPlay.Add(selected);
                    l.Remove(selected);
                    allRoles.Remove(selected);
                    c.Count--;
                }
            }
            
            Exceptions(rolesInPlay);

            foreach (KeyValuePair<IUser, Player> player in _players)
            {
                Role selected = rolesInPlay[random.Next(rolesInPlay.Count)];
                player.Value.Role = selected;
                rolesInPlay.Remove(selected);
                if (selected.Name == "Drunk")
                {
                    List<Role> t = allRoles.FindAll(r => r.Category == townsfolk);
                    Role fakeRole = t[random.Next(t.Count)];
                    player.Value.KnownRole = fakeRole;
                    allRoles.Remove(fakeRole);
                }
                else if (selected.Name == "Lunatic")
                {
                    List<Role> t = allRoles.FindAll(r => r.Category == demon);
                    Role fakeRole = t[random.Next(t.Count)];
                    player.Value.KnownRole = fakeRole;
                    allRoles.Remove(fakeRole);
                }
                else
                {
                    player.Value.KnownRole = selected;
                }
            }
        }
        public void Exceptions(List<Role> rolesInPlay)
        {
            if (rolesInPlay.Exists(r => r.Name == "Baron"))
            {
                for (int removedTownsfolk = 0; removedTownsfolk < 2; removedTownsfolk++)
                { AddOutsider(rolesInPlay); }
            }
            if (rolesInPlay.Exists(r => r.Name == "Godfather"))
            {
                if (new Random().NextDouble() > 0.5) // Coin flip to add or remove an Outsider
                { AddOutsider(rolesInPlay); }
                else
                { RemoveOutsider(rolesInPlay); }
            }
            if (rolesInPlay.Exists(r => r.Name == "Fang Gu"))
            { AddOutsider(rolesInPlay); }
            if (rolesInPlay.Exists(r => r.Name == "Vigormortis"))
            { RemoveOutsider(rolesInPlay); }

            void RemoveOutsider(List<Role> rolesInPlay)
            {
                Role toAdd;
                Role toRemove;
                var random = new Random();
                List<Role> t = rolesInPlay.FindAll(r => r.Category == outsider);
                toRemove = t[random.Next(t.Count)];
                List<Role> u = allRoles.FindAll(r => r.Category == townsfolk);
                toAdd = u[random.Next(u.Count)];

                rolesInPlay.Remove(toRemove);
                rolesInPlay.Add(toAdd);
                allRoles.Remove(toAdd);
            }

            void AddOutsider(List<Role> rolesInPlay)
            {
                Role toAdd;
                Role toRemove;
                var random = new Random();
                List<Role> t = rolesInPlay.FindAll(r => r.Category == townsfolk);
                toRemove = t[random.Next(t.Count)];
                List<Role> u = allRoles.FindAll(r => r.Category == outsider);
                toAdd = u[random.Next(u.Count)];

                rolesInPlay.Remove(toRemove);
                rolesInPlay.Add(toAdd);
                allRoles.Remove(toAdd);
            }
        }
        public void AddTB()
        {
            allRoles.Add(new Role("Washerwoman", townsfolk));
            allRoles.Add(new Role("Librarian", townsfolk));
            allRoles.Add(new Role("Investigator", townsfolk));
            allRoles.Add(new Role("Chef", townsfolk));
            allRoles.Add(new Role("Empath", townsfolk));
            allRoles.Add(new Role("Fortune Teller", townsfolk));
            allRoles.Add(new Role("Undertaker", townsfolk));
            allRoles.Add(new Role("Monk", townsfolk));
            allRoles.Add(new Role("Ravenkeeper", townsfolk));
            allRoles.Add(new Role("Virgin", townsfolk));
            allRoles.Add(new Role("Slayer", townsfolk));
            allRoles.Add(new Role("Soldier", townsfolk));
            allRoles.Add(new Role("Mayor", townsfolk));
            allRoles.Add(new Role("Butler", outsider));
            allRoles.Add(new Role("Drunk", outsider));
            allRoles.Add(new Role("Recluse", outsider));
            allRoles.Add(new Role("Saint", outsider));
            allRoles.Add(new Role("Poisoner", minion));
            allRoles.Add(new Role("Spy", minion));
            allRoles.Add(new Role("Scarlet Woman", minion));
            allRoles.Add(new Role("Baron", minion));
            allRoles.Add(new Role("Imp", demon));
        }
        public void AddBMR()
        {

            allRoles.Add(new Role("Chambermaid", townsfolk));
            allRoles.Add(new Role("Courtier", townsfolk));
            allRoles.Add(new Role("Exorcist", townsfolk));
            allRoles.Add(new Role("Fool", townsfolk));
            allRoles.Add(new Role("Gambler", townsfolk));
            allRoles.Add(new Role("Gossip", townsfolk));
            allRoles.Add(new Role("Grandmother", townsfolk));
            allRoles.Add(new Role("Innkeeper", townsfolk));
            allRoles.Add(new Role("Minstrel", townsfolk));
            allRoles.Add(new Role("Pacifist", townsfolk));
            allRoles.Add(new Role("Professor", townsfolk));
            allRoles.Add(new Role("Sailor", townsfolk));
            allRoles.Add(new Role("Tea Lady", townsfolk));
            allRoles.Add(new Role("Goon", outsider));
            allRoles.Add(new Role("Lunatic", outsider));
            allRoles.Add(new Role("Moonchild", outsider));
            allRoles.Add(new Role("Tinker", outsider));
            allRoles.Add(new Role("Assassin", minion));
            allRoles.Add(new Role("Devil's Advocate", minion));
            allRoles.Add(new Role("Godfather", minion));
            allRoles.Add(new Role("Mastermind", minion));
            allRoles.Add(new Role("Po", demon));
            allRoles.Add(new Role("Pukka", demon));
            allRoles.Add(new Role("Shabaloth", demon));
            allRoles.Add(new Role("Zombuul", demon));
        }
        public void AddSAV()
        {
            allRoles.Add(new Role("Artist", townsfolk));
            allRoles.Add(new Role("Clockmaker", townsfolk));
            allRoles.Add(new Role("Dreamer", townsfolk));
            allRoles.Add(new Role("Flowergirl", townsfolk));
            allRoles.Add(new Role("Juggler", townsfolk));
            allRoles.Add(new Role("Mathematician", townsfolk));
            allRoles.Add(new Role("Oracle", townsfolk));
            allRoles.Add(new Role("Philosopher", townsfolk));
            allRoles.Add(new Role("Sage", townsfolk));
            allRoles.Add(new Role("Savant", townsfolk));
            allRoles.Add(new Role("Seamstress", townsfolk));
            allRoles.Add(new Role("Snake Charmer", townsfolk));
            allRoles.Add(new Role("Town Crier", townsfolk));
            allRoles.Add(new Role("Barber", outsider));
            allRoles.Add(new Role("Klutz", outsider));
            allRoles.Add(new Role("Mutant", outsider));
            allRoles.Add(new Role("Sweetheart", outsider));
            allRoles.Add(new Role("Cerenovus", minion));
            allRoles.Add(new Role("Evil Twin", minion));
            allRoles.Add(new Role("Pit Hag", minion));
            allRoles.Add(new Role("Witch", minion));
            allRoles.Add(new Role("Fang Gu", demon));
            allRoles.Add(new Role("No Dashii", demon));
            allRoles.Add(new Role("Vigormortis", demon));
            allRoles.Add(new Role("Vortox", demon));
        }
    }
}