import React, { useRef, useState, useEffect } from "react";
import { Stage, Layer, Arrow, Circle } from "react-konva";
import { Agent } from "../types/Agent";
import DisplayAgents from "../components/DisplayAgents";
import DisplayTrajectories from "../components/DisplayTrajectories";

interface CanvasProps {
    agents: Agent[];
}

const Canvas: React.FC<CanvasProps> = ({ agents }) => {
    const containerRef = useRef<HTMLDivElement>(null);
    const [dimensions, setDimensions] = useState<{ width: number; height: number }>({
        width: 0,
        height: 0,
    });

    useEffect(() => {
        const checkSize = () => {
            if (containerRef.current) {
                setDimensions({
                    width: containerRef.current.offsetWidth,
                    height: containerRef.current.offsetHeight,
                });
            }
        };
        checkSize();
        window.addEventListener("resize", checkSize);
        return () => window.removeEventListener("resize", checkSize);
    }, []);

    const { width, height } = dimensions;
    const centerX = width > 0 ? width / 2 : 0;
    const centerY = height > 0 ? height / 2 : 0;
    const arrowSize = 8;
    const axisColor = "#a0a0a0";
    const agentRadius = 6;
    const agentStrokeColor = "#ffffff";

    const getKonvaCoords = (agentX: number, agentY: number) => {
        return {
            x: centerX + agentX,
            y: centerY - agentY,
        };
    };

    return (
        <div
            ref={containerRef}
            className="simulation-container"
            style={{
                width: "100%",
                height: "100%",
                overflow: "hidden",
                position: "relative",
                display: "flex",
            }}
        >
            {width > 0 && height > 0 && (
                <Stage width={width} height={height}>
                    <Layer>
                        <Arrow
                            points={[0, centerY, width - arrowSize, centerY]}
                            stroke={axisColor}
                            strokeWidth={1}
                            fill={axisColor}
                            pointerLength={arrowSize}
                            pointerWidth={arrowSize}
                        />
                        <Arrow
                            points={[centerX, height, centerX, arrowSize]}
                            stroke={axisColor}
                            strokeWidth={1}
                            fill={axisColor}
                            pointerLength={arrowSize}
                            pointerWidth={arrowSize}
                        />
                        <Circle x={centerX} y={centerY} radius={4} fill="#4dabf7" stroke="white" strokeWidth={0.5} />

                        <DisplayTrajectories agents={agents} getKonvaCoords={getKonvaCoords} />

                        <DisplayAgents
                            agents={agents}
                            getKonvaCoords={getKonvaCoords}
                            agentRadius={agentRadius}
                            agentStrokeColor={agentStrokeColor}
                        />
                    </Layer>
                </Stage>
            )}
        </div>
    );
};

export default Canvas;
