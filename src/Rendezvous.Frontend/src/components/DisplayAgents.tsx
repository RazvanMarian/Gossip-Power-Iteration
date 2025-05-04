import React from "react";
import { Circle, Text } from "react-konva";
import { Agent } from "../types/Agent";

interface DisplayAgentsProps {
    agents: Agent[];
    getKonvaCoords: (agentX: number, agentY: number) => { x: number; y: number };
    agentRadius: number;
    agentStrokeColor: string;
    showLabels?: boolean;
}

const DisplayAgents: React.FC<DisplayAgentsProps> = ({
    agents,
    getKonvaCoords,
    agentRadius,
    agentStrokeColor,
    showLabels = false,
}) => {
    return (
        <>
            {agents.map((agent) => {
                const konvaCoords = getKonvaCoords(agent.currentX, agent.currentY);

                return (
                    <React.Fragment key={agent.id}>
                        <Circle
                            x={konvaCoords.x}
                            y={konvaCoords.y}
                            radius={agentRadius}
                            // Folosim culoarea specifică a agentului!
                            fill={agent.color}
                            stroke={agentStrokeColor}
                            strokeWidth={1}
                            shadowBlur={5}
                            shadowColor="rgba(0,0,0,0.5)"
                        />
                        {showLabels && (
                            <Text
                                text={agent.id}
                                x={konvaCoords.x + agentRadius + 2}
                                y={konvaCoords.y - agentRadius}
                                fontSize={10}
                                fill={agent.color}
                            />
                        )}
                    </React.Fragment>
                );
            })}
        </>
    );
};

export default DisplayAgents;
