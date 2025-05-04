import React from "react";
import { Agent } from "../types/Agent";

interface AgentManagerProps {
    agents: Agent[];
    setAgents: React.Dispatch<React.SetStateAction<Agent[]>>;
    agentColors: string[];
    maxAgents: number;
}

const AgentManager: React.FC<AgentManagerProps> = ({ agents, setAgents, agentColors, maxAgents }) => {
    const handleAgentChange = (id: string, field: keyof Agent, value: string | number) => {
        setAgents((prevAgents) =>
            prevAgents.map((agent) => {
                if (agent.id === id && (field === "initialX" || field === "initialY")) {
                    const numValue = typeof value === "string" ? parseInt(value, 10) || 0 : value;
                    const updatedAgent = { ...agent, [field]: numValue };
                    if (field === "initialX") updatedAgent.currentX = numValue;
                    if (field === "initialY") updatedAgent.currentY = numValue;
                    updatedAgent.history = [{ x: updatedAgent.currentX, y: updatedAgent.currentY, iteration: -1 }];
                    return updatedAgent;
                }
                if (agent.id === id) {
                    return { ...agent, [field]: value };
                }
                return agent;
            })
        );
    };

    const handleDeleteAgent = (idToDelete: string) => {
        setAgents((prevAgents) => prevAgents.filter((agent) => agent.id !== idToDelete));
    };

    const handleAddAgent = () => {
        if (agents.length >= maxAgents) {
            alert(`Maximum number of agents (${maxAgents}) reached.`);
            return;
        }

        const existingIds = agents
            .map((agent) => parseInt(agent.id.replace("node", ""), 10))
            .filter((num) => !isNaN(num));

        let nextIdNumber = -1;
        for (let i = 1; i <= maxAgents; i++) {
            if (!existingIds.includes(i)) {
                nextIdNumber = i;
                break;
            }
        }

        if (nextIdNumber === -1) {
            console.error("Could not find an available agent ID.");
            return;
        }

        const nextId = `node${nextIdNumber}`;
        const nextHostPort = 5000 + nextIdNumber;

        const colorIndex = (nextIdNumber - 1) % agentColors.length;
        const newColor = agentColors[colorIndex >= 0 ? colorIndex : 0]; // Ensure index is not negative

        const newAgent: Agent = {
            id: nextId,
            hostPort: nextHostPort,
            initialX: Math.round(Math.random() * 800 - 400),
            initialY: Math.round(Math.random() * 600 - 300),
            currentX: 0,
            currentY: 0,
            history: [],
            color: newColor,
        };
        newAgent.currentX = newAgent.initialX;
        newAgent.currentY = newAgent.initialY;
        newAgent.history = [{ x: newAgent.initialX, y: newAgent.initialY, iteration: -1 }];

        setAgents((prevAgents) =>
            [...prevAgents, newAgent].sort((a, b) => {
                const numA = parseInt(a.id.replace("node", ""), 10);
                const numB = parseInt(b.id.replace("node", ""), 10);
                return numA - numB;
            })
        );
    };

    const handleInputChange = (id: string, field: "initialX" | "initialY", value: string) => {
        setAgents((prev) =>
            prev.map((a) => {
                if (a.id === id) {
                    const numValue = parseInt(value, 10);
                    return { ...a, [field]: isNaN(numValue) ? 0 : numValue };
                }
                return a;
            })
        );
    };

    return (
        <div className="agent-manager">
            <button onClick={handleAddAgent} disabled={agents.length >= maxAgents}>
                Add Agent (Max: {maxAgents})
            </button>
            <div className="agent-list">
                {agents.map((agent) => (
                    <div key={agent.id} className="agent-editor" style={{ borderColor: agent.color }}>
                        <strong>
                            {agent.id} (Port: {agent.hostPort})
                        </strong>
                        <div>
                            <label htmlFor={`${agent.id}-x`}>X:</label>
                            <input
                                type="number"
                                id={`${agent.id}-x`}
                                value={agent.initialX}
                                onBlur={(e) => handleAgentChange(agent.id, "initialX", e.target.value)}
                                onChange={(e) => handleInputChange(agent.id, "initialX", e.target.value)}
                            />
                            <span></span>
                            <label htmlFor={`${agent.id}-y`}>Y:</label>
                            <input
                                type="number"
                                id={`${agent.id}-y`}
                                value={agent.initialY}
                                onBlur={(e) => handleAgentChange(agent.id, "initialY", e.target.value)}
                                onChange={(e) => handleInputChange(agent.id, "initialY", e.target.value)}
                            />
                            <span></span>
                            <label htmlFor={`${agent.id}-color`}>Color:</label>
                            <input
                                type="color"
                                id={`${agent.id}-color`}
                                value={agent.color}
                                onChange={(e) => handleAgentChange(agent.id, "color", e.target.value)}
                            />
                            <button onClick={() => handleDeleteAgent(agent.id)} title={`Delete ${agent.id}`}>
                                &times;
                            </button>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default AgentManager;
