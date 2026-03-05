# Viking NPC (Norsemen)

Friendly vikings roam the world—working, hunting, and surviving.  
Whether they remain allies or become enemies is up to you.

---

## 0.3.0 — Overhaul

This release **completely rewrites the plugin** to address long-standing stability and performance issues.

- Settlers and Raiders replaced with **Norsemen**
- Norsemen are **tameable vikings**, friendly until provoked
- Behavior inspired by **Dvergers**, with added taming and stealing mechanics
- Many systems simplified or removed to reduce overhead

### Removed
- Raider ships
- Towns

> Towns may return in the future if this version proves stable.

---

## Features

- Tameable vikings (Norsemen)
- Configurable:
    - Random gear sets
    - Random inventory items
    - Spawn settings
    - Base health & armor
- Vikings benefit from equipped armor
- Steal from viking inventories
- Tool-based behavior:
    - Pickaxe → mining
    - Axe → lumbering
    - Fishing rod + bait → fishing
- Follow behavior:
    - Attaches to player ship
    - Teleports back when player disembarks
- Revive tamed vikings via tombstone interaction
- Ignores tameable creatures unless provoked
- Consumes inventory items to maintain happiness
- Can receive potions via hotbar use
- Equipment are excluded from dropped items on death

---

## Random Gear Sets
**YML example:**
```yml
PrefabNames:
- HelmetFlametal
- ArmorFlametalChest
- ArmorFlametalLegs
- CapeAsh
- SwordNiedhogg
- ShieldFlametalTower
RequiredDefeatKey: defeated_queen
Weight: 0.8
```

- Sets are excluded if conditions aren’t met
- `Weight` controls selection probability
- Create/Change/Delete files supported
- Files sync automatically from server

---

## Random Items

**YML example:**
```yml
PrefabName: CopperOre
RequiredDefeatKey: defeated_eikthyr
Chance: 0.5
Min: 1
Max: 5
```

- Items are skipped if conditions aren’t met
- `Chance` controls probability
- Create/Change/Delete files supported
- Files sync automatically from server

---

## Prefab IDs

- `Meadows_Norseman_RS`
- `BlackForest_Norseman_RS`
- `Swamp_Norseman_RS`
- `Mountains_Norseman_RS`
- `Plains_Norseman_RS`
- `Mistlands_Norseman_RS`
- `Ashlands_Norseman_RS`

---

## Console Commands


- `norsemen tame`: tames all nearby norsemen (admin only)
- `norsemen clear_tombs` removes all nearby norsemen tombstones (admin only)

---

## Support & Community

Questions or feedback?  
Find **Rusty** in the **Odin Plus Team Discord**:

https://discord.gg/v89DHnpvwS

Or visit **Modding Corner**:  
https://discord.gg/fB8aHSfA8B

---

## Support Development

If you enjoy this mod and want to support development:

- https://paypal.me/mpei
