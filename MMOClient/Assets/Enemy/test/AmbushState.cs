// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace BV
// {
//     public class AmbushState : State
//     {
//         public bool isSleeping;
//         public float detectionRadius = 2;
//         public string sleepAnimation;
//         public string wakeAnimation;

//         public LayerMask detectionLayer;

//         public PursueTargetState pursueTargetState;

//         public override State Tick(NewEnemyManager newEnemyManager, EnemtStats enemyStats, EnemyAnimatorManager enemyAnimatorManager)
//         {
//             if (isSleeping && newEnemyManager.isInteracting == false)
//             {
//                 enemyAnimatorManager.PlayTargetAnimation(sleepAnimation, true);
//             }

//             #region  Handle Target Detection
//             Collider[] colliders = Physics.OverlapSphere(newEnemyManager.transform.position, detectionRadius, detectionLayer);

//             for (int i = 0; i < colliders.Length; i++)
//             {
//                 CharacterManager stateManager = colliders[i].transform.GetComponent<StateManager>();

//                 if (stateManager != null)
//                 {
//                     //CHECK FOR TEAM ID
//                     Vector3 targetDirection = stateManager.transform.position - newEnemyManager.transform.position;
//                     float viewableAngle = Vector3.Angle(targetDirection, newEnemyManager.transform.forward);

//                     if (viewableAngle > newEnemyManager.minimumDetectionAngle && viewableAngle < newEnemyManager.maximumDetectionAngle)
//                     {
//                         newEnemyManager.currentTarget = stateManager;
//                         isSleeping = false;
//                         enemyAnimatorManager.PlayTargetAnimation(wakeAnimation, true);
//                     }
//                 }

//             }
//             #endregion

//             #region  Handle State Change
//             if (newEnemyManager.currentTarget != null)
//             {
//                 return pursueTargetState;
//             }
//             else
//             {
//                 return this;
//             }
//             #endregion
//         }
//     }
// }
