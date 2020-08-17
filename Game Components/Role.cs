using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordTest
{
    public class Role
    {
        private readonly string _name;
        private readonly Category _category;

        public Role(string name, Category category)
        {
            this._name = name;
            this._category = category;
        }

        public string Name
        {
            get => this._name;
        }

        public Category Category { get => this._category;  }
    }
}
