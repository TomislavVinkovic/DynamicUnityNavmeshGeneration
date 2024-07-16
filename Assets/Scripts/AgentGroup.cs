using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AgentGroup
{   
    Guid id;
    List<GameObject> agents;
    Vector3 center;

    public int AgentCount { get => agents.Count; } 
    public Guid Id { get => id; }
    public List<GameObject> Agents { get => agents; }
    public Vector3 Center { get => center; set => center = value; }

    public AgentGroup()
    {
        id = Guid.NewGuid();
        agents = new List<GameObject>();
        center = Vector3.zero;
    }

    public void AddAgent(GameObject agent)
    {
        agents.Add(agent);
    }
}
