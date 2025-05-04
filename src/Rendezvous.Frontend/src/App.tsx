import { useState } from "react";
import "./style/global.css"; // Importăm stilurile globale
import "./style/layout.css";
import "./style/components/AgentManager.css";
import "./style/components/Controls.css";
import "./style/components/Canvas.css";
import { Agent } from "./types/Agent";
import Canvas from "./components/Canvas";
import AgentManager from "./components/AgentManager";

const agentColors = [
    "#ff6b6b",
    "#4dabf7",
    "#40c057",
    "#fcc419",
    "#da77f2",
    "#ffa94d",
    "#74c0fc",
    "#69db7c",
    "#ffec99",
    "#e599f7",
];

const MAX_AGENTS = 6;

const initialAgentsData: Omit<Agent, "currentX" | "currentY" | "history" | "color">[] = [
    { id: "node1", hostPort: 5001, initialX: 50, initialY: 50 },
    { id: "node2", hostPort: 5002, initialX: 150, initialY: 50 },
];

function App() {
    const [agents, setAgents] = useState<Agent[]>(() =>
        initialAgentsData.map((data) => {
            const idNumber = parseInt(data.id.replace("node", ""), 10);
            const colorIndex = (idNumber - 1) % agentColors.length;

            const testHistory = [
                { x: data.initialX, y: data.initialY, iteration: -1 },
                { x: data.initialX + 100, y: data.initialY + 35, iteration: 0 },
                { x: data.initialX - 200, y: data.initialY + 120, iteration: 1 },
                { x: data.initialX + 20, y: data.initialY + 25, iteration: 2 },
            ];

            return {
                ...data,
                currentX: testHistory[testHistory.length - 1].x,
                currentY: testHistory[testHistory.length - 1].y,
                history: testHistory,
                color: agentColors[colorIndex >= 0 ? colorIndex : 0],
            };
        })
    );
    const [simulationStatus, setSimulationStatus] = useState<"idle" | "running" | "finished">("idle");

    const handleStartSimulation = () => {
        console.log("Starting simulation...");
        setSimulationStatus("running");
    };

    const handleResetSimulation = () => {
        console.log("Resetting simulation...");
        setAgents(
            initialAgentsData.map((data) => {
                const idNumber = parseInt(data.id.replace("node", ""), 10);
                const colorIndex = (idNumber - 1) % agentColors.length;
                const initialHistory = [{ x: data.initialX, y: data.initialY, iteration: -1 }];
                return {
                    ...data,
                    currentX: data.initialX,
                    currentY: data.initialY,
                    history: initialHistory,
                    color: agentColors[colorIndex >= 0 ? colorIndex : 0],
                };
            })
        );
        setSimulationStatus("idle");
    };

    return (
        <div className="app-container">
            <header className="app-header">
                <h1>Rendezvous Consensus Simulation</h1>
            </header>

            <main className="main-content">
                <aside className="sidebar">
                    <h2>Info / Settings</h2>
                    <AgentManager
                        agents={agents}
                        setAgents={setAgents}
                        agentColors={agentColors}
                        maxAgents={MAX_AGENTS}
                    />
                </aside>

                <section className="simulation-area">
                    <div className="controls">
                        <button onClick={handleStartSimulation} disabled={simulationStatus === "running"}>
                            Start
                        </button>
                        <button onClick={handleResetSimulation}>Reset</button>
                        <span>Status: {simulationStatus}</span>
                    </div>
                    <Canvas agents={agents} />
                </section>
            </main>
        </div>
    );
}

export default App;
