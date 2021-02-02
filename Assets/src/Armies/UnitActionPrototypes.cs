using System;
using System.Collections.Generic;

public class UnitActionPrototypes {
    private delegate UnitAction CreateActionDelegate(Unit unit);

    private static UnitActionPrototypes instance;

    private Dictionary<string, CreateActionDelegate> actions;

    private UnitActionPrototypes()
    {
        actions = new Dictionary<string, CreateActionDelegate>();

        actions.Add("flaming arrows", delegate (Unit unit) {
            return new UnitAction("Flaming Arrows", "flaming arrows", 3, 0, 2, Math.Max(2, unit.Range - 1), true, UnitAction.TargetingType.Enemy, 1.0f, 0.0f, "flaming_arrows", SpriteManager.SpriteType.Skill,
                "flaming_arrows", new AnimationData("ranged_attack"),
                delegate(Unit attacker, UnitAction action, CombatMapHex hex, bool is_preview, out AttackResult[] result, out string message) {
                    result = action.Ranged_Attack(attacker, hex.Unit, unit.Ranged_Attack.Clone.Add(new Damage(2.0f, Damage.Nature.Physical, Damage.Type.Fire)), is_preview);
                    message = null;
                    return true;
                });
        });
    }

    public static UnitActionPrototypes Instance
    {
        get {
            if(instance == null) {
                instance = new UnitActionPrototypes();
            }
            return instance;
        }
    }

    public UnitAction Get(string internal_name, Unit unit)
    {
        if(!actions.ContainsKey(internal_name)) {
            CustomLogger.Instance.Error("Action {0} does not exist", internal_name);
            return null;
        }
        return actions[internal_name](unit);
    }
}
