using System;
using System.Collections.Generic;
using Utility;


namespace Gameplay
{ 
    public class Inventory
    {
        private Dictionary<ItemType, int> items = new Dictionary<ItemType, int>(); // Item, quantity pairs

        public void Initialize()
        {
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                items.Add(itemType, 0);
            }
        }
        
        public void AddItem(ItemType item)
        {
            items.TryGetValue(item, out int amnt);
            items[item] = amnt + 1;
        }

        public bool ConsumeItem(ItemType item)
        {
            items.TryGetValue(item, out int amnt);
            if (amnt > 0)
            {
                items[item] = amnt - 1;
                return true;
            }
            return false;
        }
    }
}
