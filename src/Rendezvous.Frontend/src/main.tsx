import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./style/global.css";
import "./style/layout.css";
import "./style/components/AgentManager.css";
import "./style/components/Controls.css";
import "./style/components/Canvas.css";
import App from "./App.tsx";

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <App />
    </StrictMode>
);
