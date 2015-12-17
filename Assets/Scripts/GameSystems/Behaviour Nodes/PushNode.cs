﻿using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class PushNode : ActionNode
{
    Peloton peloton;

    // Called once when the node is created
    public virtual void Awake() { }

    // Called when the owner (BehaviourTree or ActionState) is enabled
    public override void OnEnable()
    {
        base.OnEnable();
        peloton = this.self.gameObject.GetComponent<Peloton>();
    }

    // Called when the node starts its execution
    public override void Start()
    {

    }

    // This function is called when the node is in execution
    public override Status Update()
    {
        peloton.state = Names.STATE_PUSH;
        peloton.PushFuit(peloton);
        return Status.Success;
    }

    // Called when the node ends its execution
    public override void End()
    {

    }
}