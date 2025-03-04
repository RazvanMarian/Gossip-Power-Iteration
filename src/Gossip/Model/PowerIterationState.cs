﻿namespace Gossip.Model;

public class PowerIterationState
{

    public double Weight { get; set; } = 1.0;

    public Dictionary<string, double> Bi { get; set; } = [];
}