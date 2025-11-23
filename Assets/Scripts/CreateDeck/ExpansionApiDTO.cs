using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.CreateDeck
{
    [Serializable]
    public class ExpansionApiDTO
    {
        public string code;
        public string name;
        public bool isCore;
        public bool owned;
    }
}
