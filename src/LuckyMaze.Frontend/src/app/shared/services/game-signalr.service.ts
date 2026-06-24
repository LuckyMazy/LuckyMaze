import { Injectable } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { Subject, Observable, firstValueFrom } from 'rxjs';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GameSignalRService {
  private hubConnection: HubConnection | null = null;

  // Subjects to bubble up events
  private gameStateSubject = new Subject<any>();
  private countdownTickSubject = new Subject<number>();
  private mazeGeneratedSubject = new Subject<any>();
  private aiStepSubject = new Subject<any>();
  private gameFinishedSubject = new Subject<any>();
  private connectionStatusSubject = new Subject<'Disconnected' | 'Connecting' | 'Connected'>();

  public gameState$ = this.gameStateSubject.asObservable();
  public countdownTick$ = this.countdownTickSubject.asObservable();
  public mazeGenerated$ = this.mazeGeneratedSubject.asObservable();
  public aiStep$ = this.aiStepSubject.asObservable();
  public gameFinished$ = this.gameFinishedSubject.asObservable();
  public connectionStatus$ = this.connectionStatusSubject.asObservable();

  constructor(private oidcSecurityService: OidcSecurityService) {}

  public async startConnection(): Promise<void> {
    if (this.hubConnection && this.hubConnection.state !== HubConnectionState.Disconnected) {
      return;
    }

    this.connectionStatusSubject.next('Connecting');

    try {
      // Get the JWT token from the OIDC service
      const token = await firstValueFrom(this.oidcSecurityService.getAccessToken());

      this.hubConnection = new HubConnectionBuilder()
        .withUrl(`${environment.apiBaseUrl}/hubs/game`, {
          accessTokenFactory: () => Promise.resolve(token)
        })
        .withAutomaticReconnect()
        .build();

      // Register listener events
      this.registerListeners();

      await this.hubConnection.start();
      this.connectionStatusSubject.next('Connected');
      console.log('SignalR connected to GameHub successfully.');
    } catch (err) {
      console.error('Error starting SignalR connection:', err);
      this.connectionStatusSubject.next('Disconnected');
      throw err;
    }
  }

  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop().then(() => {
        this.connectionStatusSubject.next('Disconnected');
        console.log('SignalR connection stopped.');
      });
    }
  }

  private registerListeners(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('ReceiveGameState', (state: any) => {
      this.gameStateSubject.next(state);
    });

    this.hubConnection.on('CountdownTick', (secondsRemaining: number) => {
      this.countdownTickSubject.next(secondsRemaining);
    });

    this.hubConnection.on('MazeGenerated', (mazeData: any) => {
      this.mazeGeneratedSubject.next(mazeData);
    });

    this.hubConnection.on('AiStep', (x: number, y: number, direction: string) => {
      this.aiStepSubject.next({ x, y, direction });
    });

    this.hubConnection.on('GameFinished', (winningExit: string, payouts: any) => {
      this.gameFinishedSubject.next({ winningExit, payouts });
    });

    this.hubConnection.onclose(() => {
      this.connectionStatusSubject.next('Disconnected');
    });

    this.hubConnection.onreconnecting(() => {
      this.connectionStatusSubject.next('Connecting');
    });

    this.hubConnection.onreconnected(() => {
      this.connectionStatusSubject.next('Connected');
    });
  }

  // Invokable hub commands
  public async toggleReady(isReady: boolean): Promise<void> {
    if (this.hubConnection && this.hubConnection.state === HubConnectionState.Connected) {
      await this.hubConnection.invoke('ToggleReady', isReady);
    } else {
      console.warn('Cannot invoke ToggleReady. Hub is not connected.');
    }
  }

  public async placeBet(exitName: string, amount: number): Promise<void> {
    if (this.hubConnection && this.hubConnection.state === HubConnectionState.Connected) {
      await this.hubConnection.invoke('PlaceBet', exitName, amount);
    } else {
      console.warn('Cannot invoke PlaceBet. Hub is not connected.');
    }
  }
}
