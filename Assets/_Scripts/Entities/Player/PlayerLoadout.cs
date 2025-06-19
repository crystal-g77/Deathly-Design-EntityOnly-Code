using System;
using System.Collections.Generic;
using UnityEngine;

// a class to store the player's loadout between the monster maker scene and the destruction scene
public class PlayerLoadout : MonoBehaviour 
{
    public enum WeaponSlotId
    {
        LowerLeft,
        LowerRight,
        UpperLeft,
        UpperRight,
    }

    private static bool set = false;
    [SerializeField] private static List<WeaponType> playerWeapons = new List<WeaponType>();
    [SerializeField] private static Dictionary<UpgradeType, int> playerUpgrades = new Dictionary<UpgradeType, int>();
    
    public static PlayerLoadout Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static bool isSet()
    {
        return set;
    }

    // Get the weapon for the given weapon slot or None if there isn't one set
    public static WeaponType getWeapon(WeaponSlotId slotId)
    {
        int slotIndex = (int)slotId;
        if(slotIndex < playerWeapons.Count)
        {
            return playerWeapons[slotIndex];
        }
        else
        {
            return WeaponType.None;
        }
    }

    public static int getUpgradeCount(UpgradeType upgrade)
    {
        return playerUpgrades[upgrade];
    }

    // Called when setting a new loadout
    public static void reset()
    {
        set = false;
        playerWeapons.Clear();
        playerUpgrades.Clear();
        for(int i = 0; i < Enum.GetValues(typeof(UpgradeType)).Length; ++i)
        {
            playerUpgrades[(UpgradeType)i] = 0;
        }
    }

    // Mark the loadout as complete
    public static void loadoutComplete()
    {
        set = true;
    }

    // Add an InventoryType to the current player loadout
    public static bool addInventoryToLoadout(InventoryType inventoryType)
    {
        if(inventoryType.isWeaponType())
        {
            Debug.Log("Added:" + inventoryType);
            WeaponType weaponType = inventoryType.toWeaponType();
            return addWeapon(weaponType);
        }
        else if(inventoryType.isUpgradeType())
        {
            Debug.Log("Added:" + inventoryType);
            UpgradeType upgradeType = inventoryType.toUpgradeType();
            addUpgrade(upgradeType);
            return true;
        }
        return false;
    }

    // Remove an InventoryType to the current player loadout
    public static void removeInventoryFromLoadout(InventoryType inventoryType)
    {
        if(inventoryType.isWeaponType())
        {
            Debug.Log("Removed: "+ inventoryType);
            WeaponType weaponType = inventoryType.toWeaponType();
            playerWeapons.Remove(weaponType);
        }
        else if(inventoryType.isUpgradeType())
        {
            Debug.Log("Removed: "+ inventoryType);
            UpgradeType upgradeType = inventoryType.toUpgradeType();
            removeUpgrade(upgradeType);
        }
    }

    public static int getMaxWeapons()
    {
        return Enum.GetValues(typeof(WeaponSlotId)).Length;
    }

    public static bool isValidLoadout()
    {
        return playerWeapons.Count >= 1;
    }

    public static bool getMaxWeaponsValidation(){
        return playerWeapons.Count < getMaxWeapons();
    }

    // Add the given WeaponType to the weapons list
    private static bool addWeapon(WeaponType weaponType)
    {
        if(playerWeapons.Count < getMaxWeapons())
        {
            playerWeapons.Add(weaponType);
            return true;
        }
        return false;
    }
    
    // Add the given UpgradeType to the upgrades list
    private static void addUpgrade(UpgradeType upgradeType)
    {
        playerUpgrades[upgradeType] += 1;
    }

    // Remove the given UpgradeType to the upgrades list
    private static void removeUpgrade(UpgradeType upgradeType)
    {
        playerUpgrades[upgradeType] -= 1;
    }
}
