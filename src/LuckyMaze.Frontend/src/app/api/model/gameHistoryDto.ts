export interface GameHistoryDto {
    id: string;
    state: string;
    winningExit?: string | null;
    startedAt?: string | null;
    endedAt?: string | null;
}
