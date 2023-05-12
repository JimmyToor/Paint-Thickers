using System;
using System.Collections.Generic;
using Src.Scripts.Utility;

namespace Src.Scripts.Gameplay
{ 
    public class Inventory
    {
        private Dictionary<ItemType, int> _items = new Dictionary<ItemType, int>(); // Item, quantity pairs
        
        public Inventory()
        {
            Initialize();
        }

        public void Initialize()
        {
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                _items.Add(itemType, 0);
            }
        }
        
        public void AddItem(ItemType item)
        {
            _items.TryGetValue(item, out int amnt);
            _items[item] = amnt + 1;
        }

        public bool ConsumeItem(ItemType item)
        {
            _items.TryGetValue(item, out int amnt);
            if (amnt > 0)
            {
                _items[item] = amnt - 1;
                return true;
            }
            return false;
        }
    }
}
