import { inject } from '@angular/core';
import { patchState, signalStore, withHooks, withMethods, withState } from '@ngrx/signals';
import { GameSignalRService } from '../services/game-signalr.service';
import { MazeCell } from '../../api/model/mazeCell';
import { MazeExit } from '../../api/model/mazeExit';
import { Subscription } from 'rxjs';

export type GameStoreState = {
  state: string;
  timerSeconds: number;
  players: Array<{
    displayName: string;
    balance: number;
    isReady: boolean;
    hasBet: boolean;
  }>;
  bets: Array<{
    displayName: string;
    exitName: string;
    amount: number;
  }>;
  activeMaze: {
    mazeId: string;
    width: number;
    height: number;
    gridData: Array<MazeCell>;
    exits: Array<MazeExit>;
  } | null;
  aiCurrentPosition: { x: number; y: number } | null;
  aiCurrentDirection: string | null;
  payouts: Array<{
    displayName: string;
    betAmount: number;
    payoutAmount: number;
    netProfit: number;
  }> | null;
  connectionStatus: 'Disconnected' | 'Connecting' | 'Connected';
};

export const initialGameStore: GameStoreState = {
  state: 'Idle',
  timerSeconds: 0,
  players: [],
  bets: [],
  activeMaze: null,
  aiCurrentPosition: null,
  aiCurrentDirection: null,
  payouts: null,
  connectionStatus: 'Disconnected'
};

export const GameStore = signalStore(
  { providedIn: 'root' },
  withState(initialGameStore),
  withMethods((store) => {
    const signalrService = inject(GameSignalRService);
    let subscriptions: Subscription[] = [];

    async function init(): Promise<void> {
      if (store.connectionStatus() === 'Connected' || store.connectionStatus() === 'Connecting') {
        return;
      }

      // 1. Subscribe to SignalR events
      subscriptions.push(
        signalrService.connectionStatus$.subscribe((status) => {
          patchState(store, { connectionStatus: status });
        })
      );

      subscriptions.push(
        signalrService.gameState$.subscribe((gameState) => {
          patchState(store, {
            state: gameState.state,
            timerSeconds: gameState.timerSeconds,
            players: gameState.players,
            bets: gameState.bets
          });
        })
      );

      subscriptions.push(
        signalrService.countdownTick$.subscribe((secondsRemaining) => {
          patchState(store, { timerSeconds: secondsRemaining });
        })
      );

      subscriptions.push(
        signalrService.mazeGenerated$.subscribe((mazeData) => {
          patchState(store, {
            activeMaze: mazeData,
            aiCurrentPosition: { x: Math.floor(mazeData.width / 2), y: Math.floor(mazeData.height / 2) },
            aiCurrentDirection: null,
            payouts: null
          });
        })
      );

      subscriptions.push(
        signalrService.aiStep$.subscribe((step) => {
          patchState(store, {
            aiCurrentPosition: { x: step.x, y: step.y },
            aiCurrentDirection: step.direction
          });
        })
      );

      subscriptions.push(
        signalrService.gameFinished$.subscribe((result) => {
          patchState(store, {
            payouts: result.payouts
          });
        })
      );

      // 2. Start WebSocket connection
      try {
        await signalrService.startConnection();
      } catch (err) {
        console.error('Failed to establish SignalR connection in GameStore:', err);
      }
    }

    function disconnect(): void {
      signalrService.stopConnection();
      subscriptions.forEach((sub) => sub.unsubscribe());
      subscriptions = [];
      patchState(store, initialGameStore);
    }

    async function toggleReady(isReady: boolean): Promise<void> {
      await signalrService.toggleReady(isReady);
    }

    async function placeBet(exitName: string, amount: number): Promise<void> {
      await signalrService.placeBet(exitName, amount);
    }

    return { init, disconnect, toggleReady, placeBet };
  }),
  withHooks((store) => ({
    onDestroy() {
      store.disconnect();
    }
  }))
);
