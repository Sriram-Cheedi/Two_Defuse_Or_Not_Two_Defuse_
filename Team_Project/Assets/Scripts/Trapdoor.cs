using System;
using PersistantSystems.Software.GameStateMachine.GameStates;
using UnityEngine;

public class Trapdoor : MonoBehaviour
{
    private void OnEnable()
    {
        ReadyUpState.TrapDoorOpened += Despawn;
    }

    private void OnDisable()
    {
        ReadyUpState.TrapDoorOpened -= Despawn;
    }

    private void Despawn()
    {
        gameObject.SetActive(false);
    }
}
