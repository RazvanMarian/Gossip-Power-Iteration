import React from "react";
import { Line } from "react-konva";
import { Agent } from "../types/Agent";

interface DisplayTrajectoriesProps {
    agents: Agent[];
    getKonvaCoords: (agentX: number, agentY: number) => { x: number; y: number };
}

const DisplayTrajectories: React.FC<DisplayTrajectoriesProps> = ({ agents, getKonvaCoords }) => {
    return (
        <>
            {agents.map((agent) => {
                if (!agent.history || agent.history.length < 2) {
                    return null;
                }

                const points = agent.history.flatMap((historyPoint) => {
                    const coords = getKonvaCoords(historyPoint.x, historyPoint.y);
                    return [coords.x, coords.y];
                });

                return (
                    <Line
                        key={`${agent.id}-trail`}
                        points={points}
                        stroke={agent.color}
                        strokeWidth={1.5}
                        opacity={0.6}
                        tension={0.5}
                        lineCap="round"
                        lineJoin="round"
                    />
                );
            })}
        </>
    );
};

export default DisplayTrajectories;
