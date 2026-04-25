using UnityEngine;
using System.Collections.Generic;

public class EnemyCombatAgentController : BaseCombatAgentController
{


    public EnemyCombatAgentController(
        BaseCombatAgentView baseCombatAgentView,
        ModulesFactoryCollection modulesFactoryCollection, 
        ITargetTrackerHandler targetTrackerHandler,
        UnitRotatorService unitRotatorService,
        ITargetReservationHandler targetReservationHandler)
        : base(baseCombatAgentView, modulesFactoryCollection, unitRotatorService, targetTrackerHandler, targetReservationHandler)
    {
    }
    
    public void ResetRunTimeState()
    {
    }
}
