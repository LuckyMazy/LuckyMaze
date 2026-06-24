import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideArrowLeft, lucideHistory } from '@ng-icons/lucide';
import { HlmBadge } from '@spartan-ng/helm/badge';
import { HlmButton } from '@spartan-ng/helm/button';
import { HlmCard } from '@spartan-ng/helm/card';
import { HlmSpinner } from '@spartan-ng/helm/spinner';
import { HlmTableImports } from '@spartan-ng/helm/table';
import { firstValueFrom } from 'rxjs';
import { GameService } from '../api/api/game.service';
import { GameHistoryDto } from '../api/model/gameHistoryDto';
import { ContentHeader } from '../shared/components/content-header/content-header';

@Component({
  selector: 'luckymaze-history',
  imports: [
    DatePipe,
    RouterLink,
    NgIcon,
    HlmBadge,
    HlmButton,
    HlmCard,
    HlmSpinner,
    HlmTableImports,
    ContentHeader,
  ],
  providers: [provideIcons({ lucideArrowLeft, lucideHistory })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './history.html',
})
export class HistoryComponent implements OnInit {
  private readonly gameService = inject(GameService);

  protected readonly history = signal<GameHistoryDto[]>([]);
  protected readonly isLoading = signal(true);

  async ngOnInit(): Promise<void> {
    try {
      const data = await firstValueFrom(this.gameService.apiGameHistoryGet(20));
      this.history.set(data ?? []);
    } catch (err) {
      console.error('Failed to load game history:', err);
    } finally {
      this.isLoading.set(false);
    }
  }
}
