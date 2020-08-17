using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordTest
{
    public class Category
    {
        private readonly string _name;

        public Category(string name)
        {
            this._name = name;
            this.Count = 0;
            this.TotalCount = 0;
        }

        public string Name
        {
            get => this._name;
        }

        public int Count
        {
            get;set;
        }

        public int TotalCount
        {
            get;set;
        }
    }
}
