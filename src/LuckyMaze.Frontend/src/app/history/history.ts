import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideHistory, lucideArrowLeft, lucideCheckCircle, lucideXCircle, lucideInfo } from '@ng-icons/lucide';
import { RouterLink } from '@angular/router';
import { GameService } from '../api/api/game.service';
import { GameHistoryDto } from '../api/model/gameHistoryDto';
import { ContentHeader } from '../shared/components/content-header/content-header';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'luckymaze-history',
  standalone: true,
  imports: [CommonModule, NgIcon, RouterLink, ContentHeader],
  providers: [
    provideIcons({
      lucideHistory,
      lucideArrowLeft,
      lucideCheckCircle,
      lucideXCircle,
      lucideInfo
    })
  ],
  templateUrl: './history.html'
})
export class HistoryComponent implements OnInit {
  private readonly gameService = inject(GameService);

  protected readonly history = signal<GameHistoryDto[]>([]);
  protected readonly isLoading = signal<boolean>(true);

  async ngOnInit(): Promise<void> {
    try {
      const data = await firstValueFrom(this.gameService.apiGameHistoryGet(20));
      this.history.set(data || []);
    } catch (err) {
      console.error('Failed to load game history:', err);
    } finally {
      this.isLoading.set(false);
    }
  }
}
