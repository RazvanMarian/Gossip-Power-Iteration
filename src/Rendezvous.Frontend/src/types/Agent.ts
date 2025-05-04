export interface Agent {
    id: string;
    hostPort: number;
    initialX: number;
    initialY: number;
    currentX: number;
    currentY: number;
    history: Array<{
        x: number;
        y: number;
        iteration: number;
    }>;
    color: string;
}
