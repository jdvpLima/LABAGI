using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Workshop
{
    [Serializable]
    public class InventoryGrantPayload
    {
        public long cardId;
        public short quantity;
    }
}
