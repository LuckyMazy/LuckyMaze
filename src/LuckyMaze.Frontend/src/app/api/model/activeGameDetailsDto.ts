import { MazeCell } from './mazeCell';
import { MazeExit } from './mazeExit';

export interface ActiveGameDetailsDto {
    state: string;
    timerSeconds: number;
    width?: number | null;
    height?: number | null;
    gridData?: Array<MazeCell> | null;
    exits?: Array<MazeExit> | null;
}
