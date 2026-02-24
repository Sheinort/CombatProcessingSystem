using Combat;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Collection of crude tests for combat systems.
/// </summary>
public class CombatDataBaseTests
{
    private static EntityDefinitionSO CreateDefinition(
        float[] resourceStats,
        float[] combatStats,
        float[] resistanceStats,
        float[] startingResources,
        bool leavesCorpse = false)
    {
        var def = ScriptableObject.CreateInstance<EntityDefinitionSO>();
        def.name = "Test";
        def.ResourceStats = resourceStats;
        def.CombatStats = combatStats;
        def.ResistanceStats = resistanceStats;
        def.StartingResources = startingResources;
        def.ID = int.MaxValue;
        def.LeavesCorpse = leavesCorpse;
        return def;
    }

    private static CombatWorld CreateDb(EntityDefinitionSO def, int maxEntityCount = 2, int initialBufferSize = 1)
        => new(maxEntityCount, initialBufferSize, new[] { def });

    private static OrderOfModification MakeOrder(params ResourceType[] types)
    {
        var order = new OrderOfModification();
        foreach (var t in types) order.Add(t);
        return order;
    }

    [Test]
    public void DataBaseCreation()
    {
        var maxEntityCount = 100;
        var db = new CombatWorld(maxEntityCount, 10);
        Assert.IsNotNull(db);
        Assert.AreEqual(maxEntityCount, db.StatRegistry.DeathFlags.Length);
        Assert.AreEqual(0, db.EntityMap.Count);
        db.Dispose();
    }

    [Test]
    public void EntityCreation()
    {
        var resourceStats    = new float[] { 1, 1, 2, 3 };   // MaxHealth, MaxArmor, MaxPrimaryResource, MaxSecondaryResource
        var combatStats      = new float[] { 5, 8, 13, 21 }; // AttackPower, Haste, CritChance, CritMultiplier
        var resistanceStats  = new float[] { 22, 23, 0 };    // Physical, Magical, True
        var startingResources = new float[] { 1, 1, 2, 3 };

        var def = CreateDefinition(resourceStats, combatStats, resistanceStats, startingResources);
        var db = CreateDb(def);

        var eID = CombatCommands.CreateEntity(db, def.ID);
        Debug.Log("ID: " + eID);
        Assert.AreNotEqual(new EntityID(-1), eID);
        Assert.AreEqual(0, def.ID);
        CombatCommands.ResolvePendingInits(db);
        var eIndex = db.EntityMap.GetIndex(eID);
        Debug.Log("Index: " + eIndex);

        Assert.AreEqual(resourceStats[0],   db.StatRegistry.ResourceStats[ResourceStatType.MaxHealth][eIndex].BaseValue);
        Assert.AreEqual(combatStats[1],     db.StatRegistry.CombatStats[CombatStatType.Haste][eIndex].BaseValue);
        Assert.AreEqual(startingResources[3], db.StatRegistry.Resources[(ResourceType)3][eIndex].MaxValue);
        Assert.AreEqual(startingResources[2], db.StatRegistry.Resources[(ResourceType)2][eIndex].Value);
        Assert.AreEqual(combatStats[(int)CombatStatType.AttackPower], db.StatRegistry.CombatStatBlocks[eIndex].AttackPower);

        db.Dispose();
    }

    [Test]
    public void StatChangeTest()
    {
        var resourceStats    = new float[] { 1, 1, 2, 3 };
        var combatStats      = new float[] { 5, 8, 13, 21 };
        var resistanceStats  = new float[] { 22, 23, 0 };
        var startingResources = new float[] { 1, 1, 2, 3 };

        var def = CreateDefinition(resourceStats, combatStats, resistanceStats, startingResources);
        var db = CreateDb(def);
        var eID1 = CombatCommands.CreateEntity(db, def.ID);
        var eID2 = CombatCommands.CreateEntity(db, def.ID);
        CombatCommands.ResolvePendingInits(db);

        db.ActionRegister.StatChangeRequests.Add(new StatChangeRequest
        {
            ChangeType = StatChangeType.Flat,
            OriginID = eID1, TargetID = eID2,
            Target = new StatTarget(ResourceStatType.MaxHealth),
            Value = 3
        });
        Assert.AreEqual(combatStats[0],       db.StatRegistry.CombatStatBlocks[db.EntityMap.GetIndex(eID1)].AttackPower);
        db.ActionRegister.StatChangeRequests.Add(new StatChangeRequest
        {
            ChangeType = StatChangeType.MultiplierMultiplicative,
            OriginID = eID2, TargetID = eID1,
            Target = new StatTarget(CombatStatType.AttackPower),
            Value = 2
        });
        db.ActionRegister.StatChangeRequests.Add(new StatChangeRequest
        {
            ChangeType = StatChangeType.MultiplierAdditive,
            OriginID = eID2, TargetID = eID1,
            Target = new StatTarget(ResistanceStatType.Magical),
            Value = -0.5f
        });
        IndexerSystem.Update(db);
        StatChangeSystem.Update(db);

        Assert.AreEqual(resourceStats[0] + 3,     db.StatRegistry.Resources[ResourceType.Health][db.EntityMap.GetIndex(eID2)].MaxValue);
        Assert.AreEqual(combatStats[0] * 2,       db.StatRegistry.CombatStatBlocks[db.EntityMap.GetIndex(eID1)].AttackPower);
        Assert.AreEqual(resistanceStats[1] * 0.5, db.StatRegistry.Resistances[DamageType.Magical][db.EntityMap.GetIndex(eID1)].Value);
    }

    [Test]
    public void ResourceChangeTest()
    {
        var resourceStats    = new float[] { 10, 10, 10, 10 };
        var combatStats      = new float[] { 5, 8, 13, 21 };
        var resistanceStats  = new float[] { 0, 0, 0 };
        var startingResources = new float[] { 1, 1, 2, 3 };

        var def = CreateDefinition(resourceStats, combatStats, resistanceStats, startingResources);
        var db = CreateDb(def);
        var eID1 = CombatCommands.CreateEntity(db, def.ID);
        var eID2 = CombatCommands.CreateEntity(db, def.ID);
        CombatCommands.ResolvePendingInits(db);
        
  
        var armorOrder = MakeOrder(ResourceType.Armor);
        db.ActionRegister.ResourceChangeRequests.Add(new ResourceChangeRequest
        {
            ChangeType = ResourceChangeType.FractionOfMax,
            OriginID = eID1, TargetID = eID2,
            OrderOfModification = armorOrder,
            Value = .5f
        });
        db.ActionRegister.ResourceChangeRequests.Add(new ResourceChangeRequest
        {
            ChangeType = ResourceChangeType.FractionOfCurrent,
            OriginID = eID2, TargetID = eID1,
            OrderOfModification = armorOrder,
            Value = 2f
        });
        db.ActionRegister.ResourceChangeRequests.Add(new ResourceChangeRequest
        {
            ChangeType = ResourceChangeType.Flat,
            OriginID = eID2, TargetID = eID1,
            OrderOfModification = MakeOrder(ResourceType.Health),
            Value = 3
        });
        db.ActionRegister.ResourceChangeRequests.Add(new ResourceChangeRequest
        {
            ChangeType = ResourceChangeType.Flat,
            OriginID = eID1, TargetID = eID2,
            OrderOfModification = MakeOrder(ResourceType.PrimaryResource, ResourceType.SecondaryResource),
            DamageType = DamageType.True,
            Value = -4
        });
        IndexerSystem.Update(db);
        CombatCommands.UpdateResourceSystems(db);

        Assert.AreEqual(def.StartingResources[0] + 3, db.StatRegistry.Resources[ResourceType.Health][db.EntityMap.GetIndex(eID1)].Value);
        Assert.AreEqual(0,                            db.StatRegistry.Resources[ResourceType.PrimaryResource][db.EntityMap.GetIndex(eID2)].Value);
        Assert.AreEqual(1,                            db.StatRegistry.Resources[ResourceType.SecondaryResource][db.EntityMap.GetIndex(eID2)].Value);
        Assert.AreEqual(def.StartingResources[1] + 5, db.StatRegistry.Resources[ResourceType.Armor][db.EntityMap.GetIndex(eID2)].Value);
        Assert.AreEqual(def.StartingResources[1] + def.StartingResources[1] * 2, db.StatRegistry.Resources[ResourceType.Armor][db.EntityMap.GetIndex(eID1)].Value);
    }

    [Test]
    public void DeathFlowTest()
    {
        var def = CreateDefinition(
            resourceStats: new float[] { 10, 10, 0, 0 },
            combatStats: new float[] { 0, 0, 0, 0 },
            resistanceStats: new float[] { 0, 0, 0 },
            startingResources: new float[] { 10, 10, 0, 0 },
            leavesCorpse: true);
        var db = CreateDb(def);
        var eID1 = CombatCommands.CreateEntity(db, def.ID);
        var eID2 = CombatCommands.CreateEntity(db, def.ID);
        CombatCommands.ResolvePendingInits(db);

        db.ActionRegister.ResourceChangeRequests.Add(new ResourceChangeRequest
        {
            ChangeType = ResourceChangeType.FractionOfMax,
            OriginID = eID2, TargetID = eID1,
            OrderOfModification = MakeOrder(ResourceType.Armor, ResourceType.Health),
            Value = -2
        });
        IndexerSystem.Update(db);
        CombatCommands.UpdateResourceSystems(db);
        Assert.AreEqual(0,                        db.StatRegistry.Resources[ResourceType.Health][db.EntityMap.GetIndex(eID1)].Value);
        Assert.AreEqual(def.StartingResources[0], db.StatRegistry.Resources[ResourceType.Health][db.EntityMap.GetIndex(eID2)].Value);

        DeathSystem.Update(db);
        Assert.AreEqual(1, db.DeathRegistry.DeathRequestList.Length);
        Assert.AreEqual(1, db.DeathRegistry.Corpses.Length);
        Assert.AreEqual(1, db.EntityMap.Count);
        db.Dispose();
    }

    [Test]
    public void PassiveFlowTest()
    {
        var def = CreateDefinition(
            resourceStats: new float[] { 10, 10, 0, 0 },
            combatStats: new float[] { 0, 0, 0, 0 },
            resistanceStats: new float[] { 0, 0, 0 },
            startingResources: new float[] { 10, 10, 0, 0 },
            leavesCorpse: true);
        var db = CreateDb(def);
        var eID1 = CombatCommands.CreateEntity(db, def.ID);
        var eID2 = CombatCommands.CreateEntity(db, def.ID);
        var armorOrder = MakeOrder(ResourceType.Armor);
        CombatCommands.ResolvePendingInits(db);

        var passive = new InterceptorData();
        passive.FloatValue1 = 2;
        db.InterceptorRegistry.AddToInterceptors<ShieldAbsorbInterceptor>(eID1, passive);

        db.ActionRegister.ResourceChangeRequests.Add(new ResourceChangeRequest
        {
            ChangeType = ResourceChangeType.Flat,
            OriginID = eID2, TargetID = eID1,
            OrderOfModification = armorOrder,
            Value = -4
        });
        IndexerSystem.Update(db);
        CombatCommands.UpdateResourceSystems(db);
        CombatCommands.EndFrameCleanup(db);
        Assert.AreEqual(8, db.StatRegistry.Resources[ResourceType.Armor][db.EntityMap.GetIndex(eID1)].Value);

        db.ActionRegister.ResourceChangeRequests.Add(new ResourceChangeRequest
        {
            ChangeType = ResourceChangeType.Flat,
            OriginID = eID2, TargetID = eID1,
            OrderOfModification = armorOrder,
            Value = -4
        });
        CombatCommands.UpdateResourceSystems(db);
        Assert.AreEqual(4, db.StatRegistry.Resources[ResourceType.Armor][db.EntityMap.GetIndex(eID1)].Value);
    }
    
    [Test]
    public void ChangesOverTimeTest()
    {
        var def = CreateDefinition(
            resourceStats: new float[] { 10, 10, 0, 0 },
            combatStats: new float[] { 0, 0, 0, 0 },
            resistanceStats: new float[] { 0, 0, 0 },
            startingResources: new float[] { 10, 10, 0, 0 },
            leavesCorpse: true);
        var db = CreateDb(def);
        var eID1 = CombatCommands.CreateEntity(db, def.ID);
        var eID2 = CombatCommands.CreateEntity(db, def.ID);
        var order = MakeOrder(ResourceType.Health);
        CombatCommands.ResolvePendingInits(db);

        db.OverTimeEffectsRegistry.AddResourceChangeOverTime(new ResourceChangeRequest
        {
            ChangeType = ResourceChangeType.Flat,
            OriginID = eID2, TargetID = eID1,
            OrderOfModification = order,
            Value = -1
        },
        new ResourceChangeOverTime
        {
            FrequencyPerSecond = 1,
            TimeSinceApplied = 0,
            Duration = 3
        });

        var a = db.OverTimeEffectsRegistry.ResourceChangeOverTime;
        var b = a[db.EntityMap.GetIndex(eID1)];
        Assert.AreEqual(def.StartingResources[0], db.StatRegistry.Resources[ResourceType.Health][db.EntityMap.GetIndex(eID1)].Value);

        for (int i = 0; i < 3; i++)
        {
            b.TimeSinceApplied = 1;
            b.Duration = 2 - i;
            a[db.EntityMap.GetIndex(eID1)] = b;
            ResourceChangesOverTimeSystem.Update(db);
            CombatCommands.UpdateResourceSystems(db);
            Assert.AreEqual(def.StartingResources[0] - i - 1, db.StatRegistry.Resources[ResourceType.Health][db.EntityMap.GetIndex(eID1)].Value);
            CombatCommands.EndFrameCleanup(db);
        }

        Assert.AreEqual(def.StartingResources[0] - 3, db.StatRegistry.Resources[ResourceType.Health][db.EntityMap.GetIndex(eID1)].Value);
        db.Dispose();
    }
}