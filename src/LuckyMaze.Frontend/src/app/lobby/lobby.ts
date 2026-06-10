import { Component, OnInit, OnDestroy, inject, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideCoins, lucideCheckCircle, lucideClock, lucideXCircle, lucideInfo, lucideAlertTriangle } from '@ng-icons/lucide';
import { GameStore } from '../shared/stores/game.store';
import { UserStore } from '../shared/stores/UserStore.store';
import { MazeRenderer } from './maze-renderer';
import { ContentHeader } from '../shared/components/content-header/content-header';

@Component({
  selector: 'luckymaze-lobby',
  standalone: true,
  imports: [CommonModule, FormsModule, NgIcon, MazeRenderer, ContentHeader],
  providers: [
    provideIcons({
      lucideCoins,
      lucideCheckCircle,
      lucideClock,
      lucideXCircle,
      lucideInfo,
      lucideAlertTriangle
    })
  ],
  templateUrl: './lobby.html',
  styleUrls: []
})
export class LobbyComponent implements OnInit, OnDestroy {
  protected readonly gameStore = inject(GameStore);
  protected readonly userStore = inject(UserStore);

  // Betting states
  protected selectedExit = signal<string>('');
  protected betAmount = signal<number>(100);

  // Computeds
  protected readonly currentUser = computed(() => this.userStore.currentUser());
  
  protected readonly currentInGamePlayer = computed(() => {
    const me = this.currentUser();
    if (!me) return null;
    return this.gameStore.players().find(p => p.displayName === me.displayName) || null;
  });

  protected readonly alreadyPlacedBet = computed(() => {
    const me = this.currentUser();
    if (!me) return null;
    return this.gameStore.bets().find(b => b.displayName === me.displayName) || null;
  });

  ngOnInit(): void {
    // Start SignalR connection and event listeners
    void this.gameStore.init();
    // Load local user profile to ensure balance is current
    void this.userStore.load();
  }

  ngOnDestroy(): void {
    // Clean up connection on navigate away
    this.gameStore.disconnect();
  }

  protected toggleReady(): void {
    const player = this.currentInGamePlayer();
    if (player) {
      void this.gameStore.toggleReady(!player.isReady);
    } else {
      // In case OIDC user isn't in game list yet, just toggle true
      void this.gameStore.toggleReady(true);
    }
  }

  protected selectExit(exitName: string): void {
    if (this.alreadyPlacedBet()) return;
    this.selectedExit.set(exitName);
  }

  protected setBetAmount(amount: number): void {
    if (this.alreadyPlacedBet()) return;
    this.betAmount.set(amount);
  }

  protected adjustBet(delta: number): void {
    if (this.alreadyPlacedBet()) return;
    const player = this.currentInGamePlayer();
    const balance = player?.balance ?? 1000;
    
    this.betAmount.update(val => {
      const next = val + delta;
      return Math.max(10, Math.min(next, balance));
    });
  }

  protected placeMaxBet(): void {
    if (this.alreadyPlacedBet()) return;
    const player = this.currentInGamePlayer();
    if (player) {
      this.betAmount.set(player.balance);
    }
  }

  protected submitBet(): void {
    if (this.alreadyPlacedBet()) return;
    
    const exit = this.selectedExit();
    const amount = this.betAmount();
    const player = this.currentInGamePlayer();

    if (!exit) {
      alert('Bitte wähle einen Ausgang, um deine Wette zu platzieren!');
      return;
    }

    if (amount <= 0 || (player && amount > player.balance)) {
      alert('Ungültiger Wettbetrag oder ungenügendes Guthaben!');
      return;
    }

    void this.gameStore.placeBet(exit, amount);
  }
}
