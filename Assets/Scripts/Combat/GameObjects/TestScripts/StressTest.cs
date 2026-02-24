using System;
using UnityEngine;
using Combat;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

public class StressTest : MonoBehaviour
{
    [SerializeField] private CombatLoop combatLoop;
    private CombatWorld db;
    [SerializeField] private EntityDefinitionSO definitionSo;
    private OrderOfModification _orderOrderOfModification; 
    
    private ResourceChangeRequest _baseRequestRes;
    private StatChangeRequest _baseRequestStat;
    private bool _testStarted;
    [ContextMenu("Start Test")]
    private void StartTest() => _testStarted = true;
    [ContextMenu("Stop Test")]
    private void StopTest() => _testStarted = false;
    private void Start()
    {
        db = combatLoop.combatWorld;
        _orderOrderOfModification.Add(ResourceType.Health);
        _baseRequestRes = new ResourceChangeRequest
        {
            ChangeType = ResourceChangeType.Flat,
            ChangeTypeTarget = ResourceType.Health,
            DamageType = DamageType.True,
            OrderOfModification = _orderOrderOfModification,
            Value = 1   
        };
        _baseRequestStat = new StatChangeRequest
        {
            ChangeType = StatChangeType.Flat,
            Value = 1,
            Target = new StatTarget(ResourceStatType.MaxHealth)
        };
    }
    
    
    private void Update()
    {
        if (!_testStarted) return;
        Profiler.BeginSample("Test.Creation");
        for (int i = 0; i < 1000; i++)
        {
            var id = CombatCommands.CreateEntity(db, definitionSo.ID);
            db.InterceptorRegistry.AddToInterceptors<ShieldAbsorbInterceptor>(id, new InterceptorData{FloatValue1 = 2});
        }
        Profiler.EndSample();
        Profiler.BeginSample("Test.ApplicationOfChanges");
        for (int i = 0; i < 1000; i++)
        {
            var id = new EntityID(i);
            var targetID = new EntityID(Random.Range(0, 999));
            _baseRequestRes.OriginID = id;
            _baseRequestRes.TargetID = targetID;
            _baseRequestRes.Value = 0;
            db.ActionRegister.ResourceChangeRequests.Add(_baseRequestRes);
        
            _baseRequestRes.Value = -100000;
            db.ActionRegister.ResourceChangeRequests.Add(_baseRequestRes);
            
        
            _baseRequestStat.OriginID = id;
            _baseRequestStat.TargetID = targetID;
            
            _baseRequestStat.Value = 1;
            db.ActionRegister.StatChangeRequests.Add(_baseRequestStat);
            _baseRequestStat.Value = -1;
            db.ActionRegister.StatChangeRequests.Add(_baseRequestStat);
        }
        Profiler.EndSample();
        
    }
 
}